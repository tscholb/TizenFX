using System;
using System.Diagnostics;
using Tizen.NUI.BaseComponents;
using Tizen.NUI.Components;

namespace Tizen.NUI.Samples
{
    public class WebpFittingModeSample : IExample
    {
        private const string Tag = "WEBP_FIT";
        private const string ImageDirectory = "images/WebpResume/";
        private const string PngFileName = "fitting_mode_wide.png";
        private const string WebpFileName = "fitting_mode_wide.webp";

        private Window window;
        private View root;
        private View pngFrame;
        private View webpFrame;
        private TextLabel status;
        private TextLabel pngTitle;
        private TextLabel webpTitle;
        private ImageView pngImageView;
        private ImageView webpImageView;
        private Stopwatch stopwatch;
        private Stopwatch readyWatch;
        private string resourcePath;
        private int cycleIndex;
        private FittingModeType currentFittingMode = FittingModeType.ScaleToFill;

        private sealed class FittingCase
        {
            public FittingCase(string name, string shortName, FittingModeType fittingMode)
            {
                Name = name;
                ShortName = shortName;
                FittingMode = fittingMode;
            }

            public string Name { get; }
            public string ShortName { get; }
            public FittingModeType FittingMode { get; }
        }

        private readonly FittingCase[] fittingCases = new[]
        {
            new FittingCase("ScaleToFill", "SCALE", FittingModeType.ScaleToFill),
            new FittingCase("ShrinkToFit", "SHRINK", FittingModeType.ShrinkToFit),
            new FittingCase("Fill", "FILL", FittingModeType.Fill),
            new FittingCase("FitWidth", "WIDTH", FittingModeType.FitWidth),
            new FittingCase("FitHeight", "HEIGHT", FittingModeType.FitHeight),
            new FittingCase("Center", "CENTER", FittingModeType.Center),
        };

        public void Activate()
        {
            window = NUIApplication.GetDefaultWindow();
            resourcePath = Tizen.Applications.Application.Current.DirectoryInfo.Resource;
            stopwatch = Stopwatch.StartNew();
            readyWatch = new Stopwatch();

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

            var title = CreateLabel("WebP FittingMode Sample", 24, Color.White);
            title.Size2D = new Size2D(root.Size2D.Width - 48, 42);
            root.Add(title);

            var description = CreateLabel("Default is ImageView.FittingMode ScaleToFill. ScaleToFill should crop LEFT/RIGHT; ShrinkToFit should show the full border.", 15, new Color(0.78f, 0.82f, 0.9f, 1.0f));
            description.Size2D = new Size2D(root.Size2D.Width - 48, 44);
            root.Add(description);

            root.Add(CreateButtonBar());

            var compareRow = new View
            {
                Size2D = new Size2D(root.Size2D.Width - 48, root.Size2D.Height - 270),
                Layout = new LinearLayout
                {
                    LinearOrientation = LinearLayout.Orientation.Horizontal,
                    CellPadding = new Size2D(18, 0),
                },
            };

            var frameWidth = (root.Size2D.Width - 66) / 2;
            var frameHeight = compareRow.Size2D.Height;
            compareRow.Add(CreateComparePanel("PNG / ImageVisual", frameWidth, frameHeight, out pngFrame, out pngTitle));
            compareRow.Add(CreateComparePanel("WEBP / AnimatedImageVisual", frameWidth, frameHeight, out webpFrame, out webpTitle));
            root.Add(compareRow);

            status = CreateLabel("", 15, new Color(0.85f, 0.9f, 1.0f, 1.0f));
            status.Size2D = new Size2D(root.Size2D.Width - 48, 70);
            status.MultiLine = true;
            root.Add(status);

            window.GetDefaultLayer().Add(root);
            ShowFittingMode(currentFittingMode, "activate");
            Log("Activate");
        }

        public void Deactivate()
        {
            DestroyImageViews();
            root?.Unparent();
            root?.Dispose();

            window = null;
            root = null;
            pngFrame = null;
            webpFrame = null;
            status = null;
            pngTitle = null;
            webpTitle = null;
            stopwatch = null;
            readyWatch = null;
        }

        private View CreateButtonBar()
        {
            var bar = new View
            {
                Size2D = new Size2D(root.Size2D.Width - 48, 42),
                Layout = new LinearLayout
                {
                    LinearOrientation = LinearLayout.Orientation.Horizontal,
                    CellPadding = new Size2D(8, 0),
                },
            };

            foreach (var fittingCase in fittingCases)
            {
                var button = new Button
                {
                    Size2D = new Size2D(132, 38),
                    Text = fittingCase.ShortName,
                    BackgroundColor = new Color(0.18f, 0.22f, 0.3f, 1.0f),
                };
                button.TextLabel.PointSize = 9;
                button.TextLabel.TextColor = Color.White;
                button.Clicked += (object sender, ClickedEventArgs e) =>
                {
                    ShowFittingMode(fittingCase.FittingMode, fittingCase.Name);
                };
                bar.Add(button);
            }

            return bar;
        }

        private View CreateComparePanel(string title, int width, int height, out View frame, out TextLabel titleLabel)
        {
            var panel = new View
            {
                Size2D = new Size2D(width, height),
                Layout = new LinearLayout
                {
                    LinearOrientation = LinearLayout.Orientation.Vertical,
                    CellPadding = new Size2D(0, 8),
                    Padding = new Extents(0, 0, 0, 0),
                },
            };

            titleLabel = CreateLabel(title, 16, Color.White);
            titleLabel.Size2D = new Size2D(width, 32);
            panel.Add(titleLabel);

            frame = new View
            {
                Size2D = new Size2D(width, height - 40),
                BackgroundColor = new Color(0.16f, 0.16f, 0.2f, 1.0f),
            };
            panel.Add(frame);

            return panel;
        }

        private void ShowFittingMode(FittingModeType fittingMode, string reason)
        {
            if (pngFrame == null || webpFrame == null)
            {
                return;
            }

            cycleIndex++;
            currentFittingMode = fittingMode;
            readyWatch.Restart();
            DestroyImageViews();

            pngImageView = CreateImageView("PNG", pngFrame.Size2D);
            pngImageView.Image = CreatePngVisualMap();
            pngImageView.FittingMode = fittingMode;
            pngFrame.Add(pngImageView);

            webpImageView = CreateImageView("WEBP", webpFrame.Size2D);
            webpImageView.Image = CreateWebpVisualMap();
            webpImageView.FittingMode = fittingMode;
            webpFrame.Add(webpImageView);
            webpImageView.Play();

            pngTitle.Text = $"PNG / ImageVisual / {fittingMode}";
            webpTitle.Text = $"WEBP / AnimatedImageVisual / {fittingMode}";
            UpdateStatus(reason);
            Log($"Show cycle={cycleIndex}, reason={reason}, fitting={fittingMode}");
        }

        private ImageView CreateImageView(string name, Size2D size)
        {
            var imageView = new ImageView
            {
                Name = name,
                Size2D = new Size2D(size.Width, size.Height),
                SynchronousLoading = true,
            };
            imageView.ResourceReady += OnResourceReady;
            return imageView;
        }

        private PropertyMap CreatePngVisualMap()
        {
            var visual = new ImageVisual
            {
                URL = resourcePath + ImageDirectory + PngFileName,
                ReleasePolicy = ReleasePolicyType.Destroyed,
                SynchronousLoading = true,
            };
            return visual.OutputVisualMap;
        }

        private PropertyMap CreateWebpVisualMap()
        {
            var visual = new AnimatedImageVisual
            {
                URL = resourcePath + ImageDirectory + WebpFileName,
                BatchSize = 2,
                CacheSize = 2,
            };

            PropertyMap map = visual.OutputVisualMap;
            map.Insert(ImageVisualProperty.ReleasePolicy, new PropertyValue((int)ReleasePolicyType.Destroyed));
            map.Insert(ImageVisualProperty.SynchronousLoading, new PropertyValue(true));
            return map;
        }

        private void DestroyImageViews()
        {
            DestroyImageView(ref pngImageView);
            DestroyImageView(ref webpImageView);
        }

        private void DestroyImageView(ref ImageView imageView)
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
            bool pngReady = pngImageView != null && pngImageView.IsResourceReady();
            bool webpReady = webpImageView != null && webpImageView.IsResourceReady();
            status.Text =
                $"Last: {reason}\n" +
                $"Cycle: {cycleIndex}, ImageView.FittingMode: {currentFittingMode}\n" +
                $"PNG ready: {pngReady}, WEBP ready: {webpReady}";
        }

        private void OnResourceReady(object sender, ImageView.ResourceReadyEventArgs e)
        {
            var imageView = sender as ImageView;
            Log($"ResourceReady cycle={cycleIndex}, target={imageView?.Name}, status={imageView?.LoadingStatus}, fitting={currentFittingMode}, elapsed={readyWatch.ElapsedMilliseconds}ms");
            UpdateStatus("resource ready");
        }

        private void Log(string message)
        {
            long elapsed = stopwatch?.ElapsedMilliseconds ?? 0;
            Tizen.Log.Fatal(Tag, $"[{elapsed}ms] {message}");
        }
    }
}
