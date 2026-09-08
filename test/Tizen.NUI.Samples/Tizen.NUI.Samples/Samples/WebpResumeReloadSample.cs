using System;
using System.Diagnostics;
using Tizen.NUI.BaseComponents;
using Tizen.NUI.Components;

namespace Tizen.NUI.Samples
{
    public class WebpResumeReloadSample : IExample
    {
        private const string Tag = "WEBP_RELOAD";
        private Window window;
        private View root;
        private View imageHost;
        private TextLabel title;
        private TextLabel status;
        private View buttonBar;
        private ImageView imageView;
        private Stopwatch stopwatch;
        private Stopwatch readyWatch = new Stopwatch();
        private string resourcePath;
        private bool hostAttached;
        private VisualCase currentCase;
        private VisualCase[] visualCases;
        private int cycleIndex;
        private string currentFormat = "";

        private sealed class VisualCase
        {
            public VisualCase(string name, string shortName, string fileName, bool isWebp)
            {
                Name = name;
                ShortName = shortName;
                FileName = fileName;
                IsWebp = isWebp;
            }

            public string Name { get; }
            public string ShortName { get; }
            public string FileName { get; }
            public bool IsWebp { get; }
        }

        public void Activate()
        {
            window = NUIApplication.GetDefaultWindow();
            resourcePath = Tizen.Applications.Application.Current.DirectoryInfo.Resource;
            stopwatch = Stopwatch.StartNew();
            visualCases = CreateVisualCases();

            root = new View
            {
                Size2D = new Size2D(window.Size.Width, window.Size.Height),
                BackgroundColor = new Color(0.08f, 0.08f, 0.1f, 1.0f),
                Layout = new LinearLayout
                {
                    LinearOrientation = LinearLayout.Orientation.Vertical,
                    CellPadding = new Size2D(0, 12),
                    Padding = new Extents(24, 24, 24, 24),
                },
            };

            title = CreateLabel("WebP Format Load Sample", 24, Color.White);
            title.Size2D = new Size2D(root.Size2D.Width - 48, 46);
            root.Add(title);

            var description = CreateLabel("Press a format button to load one full-window image. WebP variants use AnimatedImageVisual; PNG and JPEG use ImageVisual. Desired size is not set.", 15, new Color(0.78f, 0.82f, 0.9f, 1.0f));
            description.Size2D = new Size2D(root.Size2D.Width - 48, 58);
            description.MultiLine = true;
            root.Add(description);

            buttonBar = CreateButtonBar();
            root.Add(buttonBar);

            imageHost = new View
            {
                Size2D = new Size2D(window.Size.Width, window.Size.Height),
                BackgroundColor = new Color(0.16f, 0.16f, 0.2f, 1.0f),
                Layout = new LinearLayout
                {
                    LinearOrientation = LinearLayout.Orientation.Vertical,
                    CellPadding = new Size2D(0, 0),
                    Padding = new Extents(0, 0, 0, 0),
                },
            };

            status = CreateLabel("", 16, new Color(0.85f, 0.9f, 1.0f, 1.0f));
            status.Size2D = new Size2D(root.Size2D.Width - 48, 84);
            status.MultiLine = true;
            root.Add(status);

            window.GetDefaultLayer().Add(root);
            UpdateStatus("ready");
            Log("Activate");
        }

        public void Deactivate()
        {
            DestroyImageView();

            imageHost?.Unparent();
            root?.Unparent();
            root?.Dispose();

            imageHost = null;
            root = null;
            title = null;
            buttonBar = null;
            status = null;
            window = null;
            stopwatch = null;
            hostAttached = false;
        }

        private void RequestShowCase(int caseIndex)
        {
            ShowCase(caseIndex, "button");
        }

        private void ShowCase(int caseIndex, string reason)
        {
            if (root == null || imageHost == null || visualCases == null || caseIndex < 0 || caseIndex >= visualCases.Length)
            {
                return;
            }

            cycleIndex++;
            currentCase = visualCases[caseIndex];
            currentFormat = currentCase.Name;

            if (!hostAttached)
            {
                root.Add(imageHost);
                hostAttached = true;
            }

            DestroyImageView();
            imageView = new ImageView
            {
                Size2D = new Size2D(window.Size.Width, window.Size.Height),
                FittingMode = FittingModeType.ScaleToFill,
                SynchronousLoading = true,
            };
            imageView.ResourceReady += OnResourceReady;
            imageHost.Add(imageView);

            readyWatch.Restart();
            imageView.Image = currentCase.IsWebp ? CreateWebpVisualMap(currentCase.FileName) : CreateImageVisualMap(currentCase.FileName);
            if (currentCase.IsWebp)
            {
                imageView.Play();
            }

            Log($"Show cycle={cycleIndex}, format={currentFormat}, reason={reason}, load start");
            UpdateStatus("show");

        }

        private View CreateButtonBar()
        {
            const int buttonsPerRow = 7;
            var bar = new View
            {
                Size2D = new Size2D(root.Size2D.Width - 48, 186),
                Layout = new LinearLayout
                {
                    LinearOrientation = LinearLayout.Orientation.Vertical,
                    CellPadding = new Size2D(0, 4),
                },
            };

            var controlRow = CreateButtonRow();
            var hideButton = CreateButton("HIDE");
            hideButton.Clicked += (object sender, ClickedEventArgs e) =>
            {
                HideCurrentImage();
            };
            controlRow.Add(hideButton);
            bar.Add(controlRow);

            View row = null;
            for (int i = 0; i < visualCases.Length; i++)
            {
                if (i % buttonsPerRow == 0)
                {
                    row = CreateButtonRow();
                    bar.Add(row);
                }

                int caseIndex = i;
                var button = CreateButton(visualCases[i].ShortName);
                button.Clicked += (object sender, ClickedEventArgs e) =>
                {
                    RequestShowCase(caseIndex);
                };
                row.Add(button);
            }

            return bar;
        }

        private View CreateButtonRow()
        {
            return new View
            {
                Size2D = new Size2D(root.Size2D.Width - 48, 34),
                Layout = new LinearLayout
                {
                    LinearOrientation = LinearLayout.Orientation.Horizontal,
                    CellPadding = new Size2D(6, 0),
                },
            };
        }

        private Button CreateButton(string text)
        {
            var button = new Button
            {
                Size2D = new Size2D(92, 34),
                Text = text,
                BackgroundColor = new Color(0.18f, 0.22f, 0.3f, 1.0f),
            };
            button.TextLabel.PointSize = 9;
            button.TextLabel.TextColor = Color.White;
            return button;
        }

        private void HideCurrentImage()
        {
            DestroyImageView();

            if (hostAttached && root != null && imageHost != null)
            {
                root.Remove(imageHost);
                hostAttached = false;
            }

            Log($"Hide cycle={cycleIndex}, format={currentFormat}, reason=button");
            UpdateStatus("hide");
        }

        private VisualCase[] CreateVisualCases()
        {
            return new[]
            {
                new VisualCase("PNG_RGBA_BASELINE", "PNG", "FH_EndAlert_AlertBG.png", false),
                new VisualCase("JPEG_Q75_BASELINE", "J75", "FH_EndAlert_AlertBG_q75.jpg", false),
                new VisualCase("JPEG_Q90_BASELINE", "J90", "FH_EndAlert_AlertBG_q90.jpg", false),
                new VisualCase("JPEG_Q92_BASELINE", "J92", "FH_EndAlert_AlertBG_q92.jpg", false),
                new VisualCase("JPEG_Q95_BASELINE", "J95", "FH_EndAlert_AlertBG_q95.jpg", false),
                new VisualCase("JPEG_Q97_BASELINE", "J97", "FH_EndAlert_AlertBG_q97.jpg", false),
                new VisualCase("JPEG_Q99_BASELINE", "J99", "FH_EndAlert_AlertBG_q99.jpg", false),
                new VisualCase("WEBP_LOSSY_ALPHA_ORIGINAL", "ORG", "FH_EndAlert_AlertBG.webp", true),
                new VisualCase("WEBP_LOSSY_ALPHA_Q30", "A30", "FH_EndAlert_AlertBG_lossy_alpha_q30.webp", true),
                new VisualCase("WEBP_LOSSY_ALPHA_Q50", "A50", "FH_EndAlert_AlertBG_lossy_alpha_q50.webp", true),
                new VisualCase("WEBP_LOSSY_ALPHA_Q75", "A75", "FH_EndAlert_AlertBG_lossy_alpha_q75.webp", true),
                new VisualCase("WEBP_LOSSY_ALPHA_Q85", "A85", "FH_EndAlert_AlertBG_lossy_alpha_q85.webp", true),
                new VisualCase("WEBP_LOSSY_ALPHA_Q90", "A90", "FH_EndAlert_AlertBG_lossy_alpha_q90.webp", true),
                new VisualCase("WEBP_LOSSY_ALPHA_Q92", "A92", "FH_EndAlert_AlertBG_lossy_alpha_q92.webp", true),
                new VisualCase("WEBP_LOSSY_ALPHA_Q95", "A95", "FH_EndAlert_AlertBG_lossy_alpha_q95.webp", true),
                new VisualCase("WEBP_LOSSY_ALPHA_Q97", "A97", "FH_EndAlert_AlertBG_lossy_alpha_q97.webp", true),
                new VisualCase("WEBP_LOSSY_ALPHA_Q99", "A99", "FH_EndAlert_AlertBG_lossy_alpha_q99.webp", true),
                new VisualCase("WEBP_LOSSY_NO_ALPHA_Q30", "N30", "FH_EndAlert_AlertBG_lossy_noalpha_q30.webp", true),
                new VisualCase("WEBP_LOSSY_NO_ALPHA_Q50", "N50", "FH_EndAlert_AlertBG_lossy_noalpha_q50.webp", true),
                new VisualCase("WEBP_LOSSY_NO_ALPHA_Q75", "N75", "FH_EndAlert_AlertBG_lossy_noalpha_q75.webp", true),
                new VisualCase("WEBP_LOSSY_NO_ALPHA_Q85", "N85", "FH_EndAlert_AlertBG_lossy_noalpha_q85.webp", true),
                new VisualCase("WEBP_LOSSY_NO_ALPHA_Q90", "N90", "FH_EndAlert_AlertBG_lossy_noalpha_q90.webp", true),
                new VisualCase("WEBP_LOSSY_NO_ALPHA_Q92", "N92", "FH_EndAlert_AlertBG_lossy_noalpha_q92.webp", true),
                new VisualCase("WEBP_LOSSY_NO_ALPHA_Q95", "N95", "FH_EndAlert_AlertBG_lossy_noalpha_q95.webp", true),
                new VisualCase("WEBP_LOSSY_NO_ALPHA_Q97", "N97", "FH_EndAlert_AlertBG_lossy_noalpha_q97.webp", true),
                new VisualCase("WEBP_LOSSY_NO_ALPHA_Q99", "N99", "FH_EndAlert_AlertBG_lossy_noalpha_q99.webp", true),
                new VisualCase("WEBP_LOSSLESS_ALPHA", "LL-A", "FH_EndAlert_AlertBG_lossless_alpha.webp", true),
                new VisualCase("WEBP_LOSSLESS_NO_ALPHA", "LL", "FH_EndAlert_AlertBG_lossless_noalpha.webp", true),
            };
        }

        private PropertyMap CreateWebpVisualMap(string fileName)
        {
            var visual = new AnimatedImageVisual
            {
                URL = resourcePath + "images/WebpResume/" + fileName,
                BatchSize = 2,
                CacheSize = 2,
            };

            PropertyMap map = visual.OutputVisualMap;
            map.Insert(ImageVisualProperty.ReleasePolicy, new PropertyValue((int)ReleasePolicyType.Destroyed));
            map.Insert(ImageVisualProperty.SynchronousLoading, new PropertyValue(true));
            return map;
        }

        private PropertyMap CreateImageVisualMap(string fileName)
        {
            var visual = new ImageVisual
            {
                URL = resourcePath + "images/WebpResume/" + fileName,
                ReleasePolicy = ReleasePolicyType.Destroyed,
                SynchronousLoading = true,
                FittingMode = FittingModeType.ScaleToFill,
            };
            return visual.OutputVisualMap;
        }

        private void DestroyImageView()
        {
            if (imageView == null)
            {
                return;
            }

            imageView.ResourceReady -= OnResourceReady;
            imageView.Unparent();
            imageView.Dispose();
            imageView = null;
        }

        private TextLabel CreateLabel(string text, float pointSize, Color color)
        {
            return new TextLabel
            {
                Text = text,
                PointSize = pointSize,
                TextColor = color,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Begin,
                WidthResizePolicy = ResizePolicyType.FillToParent,
            };
        }

        private void UpdateStatus(string reason)
        {
            if (status == null)
            {
                return;
            }

            bool ready = imageView != null && imageView.IsResourceReady();
            status.Text =
                $"Last: {reason}\n" +
                $"Cycle: {cycleIndex}, current: {currentFormat}, visible: {hostAttached}\n" +
                $"ResourceReady: {ready}";
        }

        private void OnResourceReady(object sender, ImageView.ResourceReadyEventArgs e)
        {
            Log($"ResourceReady cycle={cycleIndex}, format={currentFormat}, status={imageView.LoadingStatus}, elapsed={readyWatch.ElapsedMilliseconds}ms");
            UpdateStatus("resource ready");
        }

        private void Log(string message)
        {
            long elapsed = stopwatch?.ElapsedMilliseconds ?? 0;
            Tizen.Log.Fatal(Tag, $"[{elapsed}ms] {message}");
        }
    }
}
