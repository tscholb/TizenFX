using System;
using Tizen.NUI.BaseComponents;

namespace Tizen.NUI.Samples
{
    /// <summary>
    /// Reproduces the WebP desired-size fitting regression.
    /// The source image is 600x300. With FitKeepAspectRatio, the circle must stay round
    /// and the image must occupy a 2:1 area centered inside each square red view.
    /// </summary>
    public class WebpFittingModeRegressionTest : IExample
    {
        private const int DesiredSize = 300;
        private const int CaseCount = 5;
        private const string LogTag = "WEBP_FITTING_SAMPLE";

        private View root;
        private ImageView desiredWebpView;
        private TextLabel desiredWebpLabel;
        private bool desiredWebpUsesFitMode = true;

        public void Activate()
        {
            Window window = NUIApplication.GetDefaultWindow();
            Size2D windowSize = window.Size;

            const int topAreaHeight = 60;
            const int labelHeight = 34;
            const int rowGap = 6;
            int imageSize = Math.Min(300, Math.Min(windowSize.Width - 40, (windowSize.Height - topAreaHeight - (labelHeight + rowGap) * CaseCount) / CaseCount));
            imageSize = Math.Max(100, imageSize);
            int rowHeight = imageSize + labelHeight + rowGap;
            int contentHeight = rowHeight * CaseCount;
            int startY = topAreaHeight + Math.Max(0, (windowSize.Height - topAreaHeight - contentHeight) / 2);
            int startX = Math.Max(0, (windowSize.Width - imageSize) / 2);

            string resourcePath = Tizen.Applications.Application.Current.DirectoryInfo.Resource;
            string webpPath = resourcePath + "images/WebpFittingModeRegression/source-600x300.webp";
            string pngPath = resourcePath + "images/WebpFittingModeRegression/source-600x300.png";

            root = new View
            {
                WidthResizePolicy = ResizePolicyType.FillToParent,
                HeightResizePolicy = ResizePolicyType.FillToParent,
                BackgroundColor = new Color(0.08f, 0.08f, 0.08f, 1.0f),
            };
            window.GetDefaultLayer().Add(root);

            TextLabel guide = new TextLabel
            {
                Text = "Expected: circles stay round. FitKeep shows red bars; OverFit fills and crops. Tap the second image for late Fill.",
                Position2D = new Position2D(10, 8),
                Size2D = new Size2D(windowSize.Width - 20, 48),
                MultiLine = true,
                PointSize = 14.0f,
                TextColor = Color.White,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };
            root.Add(guide);

            AddCase("WebP / desired unset (baseline)", webpPath, false, startX, startY, imageSize);

            desiredWebpView = AddCase(
                "WebP / desired 300x300 / FitKeep (creation)",
                webpPath,
                true,
                startX,
                startY + rowHeight,
                imageSize,
                out desiredWebpLabel);
            desiredWebpView.TouchEvent += OnDesiredWebpTouched;

            AddCase(
                "WebP / desired 300x300 / OverFit (creation)",
                webpPath,
                true,
                VisualFittingModeType.OverFitKeepAspectRatio,
                startX,
                startY + rowHeight * 2,
                imageSize);

            AddCase(
                "PNG / desired 300x300 / FitKeep (control)",
                pngPath,
                true,
                startX,
                startY + rowHeight * 3,
                imageSize);

            AddCase(
                "PNG / desired 300x300 / OverFit",
                pngPath,
                true,
                VisualFittingModeType.OverFitKeepAspectRatio,
                startX,
                startY + rowHeight * 4,
                imageSize);

            Tizen.Log.Info(LogTag, $"source=600x300 view={imageSize}x{imageSize} desired={DesiredSize}x{DesiredSize}");
        }

        public void Deactivate()
        {
            if (desiredWebpView != null)
            {
                desiredWebpView.TouchEvent -= OnDesiredWebpTouched;
            }

            root?.Unparent();
            root?.Dispose();
            root = null;
            desiredWebpView = null;
            desiredWebpLabel = null;
        }

        private ImageView AddCase(string title, string url, bool setDesiredSize, int x, int y, int imageSize)
        {
            return AddCase(title, url, setDesiredSize, VisualFittingModeType.FitKeepAspectRatio, x, y, imageSize, out _);
        }

        private ImageView AddCase(string title, string url, bool setDesiredSize, int x, int y, int imageSize, out TextLabel label)
        {
            return AddCase(title, url, setDesiredSize, VisualFittingModeType.FitKeepAspectRatio, x, y, imageSize, out label);
        }

        private ImageView AddCase(string title, string url, bool setDesiredSize, VisualFittingModeType visualFittingMode, int x, int y, int imageSize)
        {
            return AddCase(title, url, setDesiredSize, visualFittingMode, x, y, imageSize, out _);
        }

        private ImageView AddCase(string title, string url, bool setDesiredSize, VisualFittingModeType visualFittingMode, int x, int y, int imageSize, out TextLabel label)
        {
            label = new TextLabel
            {
                Text = title,
                Position2D = new Position2D(x, y),
                Size2D = new Size2D(imageSize, 34),
                PointSize = 13.0f,
                TextColor = Color.White,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };
            root.Add(label);

            ImageView imageView = new ImageView
            {
                Position2D = new Position2D(x, y + 34),
                Size2D = new Size2D(imageSize, imageSize),
                BackgroundColor = new Color(0.5f, 0.0f, 0.0f, 1.0f),
            };

            using (PropertyMap imageMap = CreateImageMap(url, setDesiredSize, visualFittingMode))
            {
                imageView.Image = imageMap;
            }

            root.Add(imageView);
            return imageView;
        }

        private static PropertyMap CreateImageMap(string url, bool setDesiredSize, VisualFittingModeType visualFittingMode)
        {
            PropertyMap map = new PropertyMap();
            Insert(map, Visual.Property.Type, (int)Visual.Type.Image);
            Insert(map, ImageVisualProperty.URL, url);
            Insert(map, Visual.Property.VisualFittingMode, (int)visualFittingMode);

            if (setDesiredSize)
            {
                Insert(map, ImageVisualProperty.DesiredWidth, DesiredSize);
                Insert(map, ImageVisualProperty.DesiredHeight, DesiredSize);
            }

            return map;
        }

        private static void Insert(PropertyMap map, int key, int value)
        {
            using PropertyValue propertyValue = new PropertyValue(value);
            map.Insert(key, propertyValue);
        }

        private static void Insert(PropertyMap map, int key, string value)
        {
            using PropertyValue propertyValue = new PropertyValue(value);
            map.Insert(key, propertyValue);
        }

        private bool OnDesiredWebpTouched(object sender, View.TouchEventArgs e)
        {
            if (e.Touch.GetState(0) != PointStateType.Down)
            {
                return false;
            }

            desiredWebpUsesFitMode = !desiredWebpUsesFitMode;
            desiredWebpView.FittingMode = desiredWebpUsesFitMode
                ? FittingModeType.ShrinkToFit
                : FittingModeType.Fill;

            desiredWebpLabel.Text = desiredWebpUsesFitMode
                ? "WebP / desired 300x300 / late ShrinkToFit"
                : "WebP / desired 300x300 / late Fill";

            Tizen.Log.Info(LogTag, $"late FittingMode={desiredWebpView.FittingMode}");
            return true;
        }
    }
}
