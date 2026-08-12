using System;
using System.Text;
using Tizen.NUI;
using Tizen.NUI.BaseComponents;

namespace Tizen.NUI.Samples
{
    public class FrameUpdateCallbackToggleTest : IExample
    {
        private const int ItemCount = 9;
        private const float ViewportWidth = 640.0f;
        private const float ViewportHeight = 170.0f;
        private const float ItemSize = 90.0f;
        private const float ItemGap = 28.0f;
        private const float ItemStartX = 22.0f;
        private const float ItemY = 40.0f;
        private const float MoveStep = 35.0f;

        private readonly Color[] itemColors =
        {
            new Color(0.90f, 0.12f, 0.10f, 1.0f),
            new Color(0.12f, 0.42f, 0.86f, 1.0f),
            new Color(0.12f, 0.62f, 0.35f, 1.0f),
            new Color(0.94f, 0.64f, 0.12f, 1.0f),
            new Color(0.55f, 0.25f, 0.78f, 1.0f),
            new Color(0.05f, 0.62f, 0.70f, 1.0f),
            new Color(0.86f, 0.25f, 0.42f, 1.0f),
            new Color(0.38f, 0.46f, 0.12f, 1.0f),
            new Color(0.30f, 0.30f, 0.34f, 1.0f),
        };

        private Window window;
        private View viewport;
        private View content;
        private TextLabel statusLabel;
        private TextLabel helpLabel;
        private View leftBoundary;
        private View rightBoundary;
        private View[] items;
        private TextLabel[] itemLabels;
        private float scrollOffset;
        private bool appIgnoredCulling;

        public void Activate()
        {
            window = NUIApplication.GetDefaultWindow();
            window.BackgroundColor = Color.White;
            window.KeyEvent += OnKeyEvent;

            CreateViewport();
            CreateStatusLabels();
            CreateItems();
            UpdateScene();
        }

        public void Deactivate()
        {
            if (window != null)
            {
                window.KeyEvent -= OnKeyEvent;
            }

            DisposeView(ref statusLabel);
            DisposeView(ref helpLabel);
            DisposeView(ref leftBoundary);
            DisposeView(ref rightBoundary);

            if (items != null)
            {
                for (int i = 0; i < items.Length; i++)
                {
                    if (items[i] != null)
                    {
                        items[i].Ignored = false;
                        items[i].Unparent();
                        items[i].Dispose();
                        items[i] = null;
                    }
                }
                items = null;
            }

            if (content != null)
            {
                content.Unparent();
                content.Dispose();
                content = null;
            }

            if (viewport != null)
            {
                viewport.Unparent();
                viewport.Dispose();
                viewport = null;
            }

            itemLabels = null;
            window = null;
        }

        private void CreateViewport()
        {
            viewport = new View()
            {
                Name = "ignored-culling-viewport",
                Size = new Size(ViewportWidth, ViewportHeight),
                ParentOrigin = ParentOrigin.Center,
                PivotPoint = PivotPoint.Center,
                PositionUsesPivotPoint = true,
                Position = new Position(0.0f, -30.0f, 0.0f),
                BackgroundColor = new Color(0.93f, 0.95f, 0.96f, 1.0f),
                ClippingMode = ClippingModeType.ClipToBoundingBox,
            };
            window.GetDefaultLayer().Add(viewport);

            float windowWidth = window.Size.Width;
            float windowHeight = window.Size.Height;
            float boundaryY = (windowHeight - ViewportHeight) * 0.5f - 30.0f;
            float leftX = (windowWidth - ViewportWidth) * 0.5f;
            float rightX = leftX + ViewportWidth - 3.0f;

            leftBoundary = CreateBoundary(leftX, boundaryY);
            rightBoundary = CreateBoundary(rightX, boundaryY);

            content = new View()
            {
                Name = "ignored-culling-content",
                Size = new Size(ItemStartX + ItemCount * (ItemSize + ItemGap), ViewportHeight),
                ParentOrigin = ParentOrigin.TopLeft,
                PivotPoint = PivotPoint.TopLeft,
                PositionUsesPivotPoint = true,
                Position = new Position(0.0f, 0.0f, 0.0f),
            };
            viewport.Add(content);
        }

        private View CreateBoundary(float x, float y)
        {
            View boundary = new View()
            {
                Size = new Size(3.0f, ViewportHeight),
                ParentOrigin = ParentOrigin.TopLeft,
                PivotPoint = PivotPoint.TopLeft,
                PositionUsesPivotPoint = true,
                Position = new Position(x, y, 0.0f),
                BackgroundColor = Color.Black,
            };
            window.GetDefaultLayer().Add(boundary);
            return boundary;
        }

        private void CreateStatusLabels()
        {
            helpLabel = new TextLabel("Left/Right: move  |  0: reset  |  I: toggle app Ignored culling  |  C: clear Ignored")
            {
                Size = new Size(window.Size.Width, 42.0f),
                ParentOrigin = ParentOrigin.TopLeft,
                PivotPoint = PivotPoint.TopLeft,
                PositionUsesPivotPoint = true,
                Position = new Position(20.0f, 20.0f, 0.0f),
                TextColor = Color.Black,
                HorizontalAlignment = HorizontalAlignment.Begin,
                VerticalAlignment = VerticalAlignment.Center,
                PointSize = 8,
            };
            window.GetDefaultLayer().Add(helpLabel);

            statusLabel = new TextLabel()
            {
                Size = new Size(window.Size.Width - 40.0f, 190.0f),
                ParentOrigin = ParentOrigin.BottomLeft,
                PivotPoint = PivotPoint.BottomLeft,
                PositionUsesPivotPoint = true,
                Position = new Position(20.0f, -20.0f, 0.0f),
                TextColor = Color.Black,
                HorizontalAlignment = HorizontalAlignment.Begin,
                VerticalAlignment = VerticalAlignment.Bottom,
                PointSize = 7,
            };
            window.GetDefaultLayer().Add(statusLabel);
        }

        private void CreateItems()
        {
            items = new View[ItemCount];
            itemLabels = new TextLabel[ItemCount];

            for (int i = 0; i < ItemCount; i++)
            {
                View item = new View()
                {
                    Name = $"ignored-culling-item-{i}",
                    Size = new Size(ItemSize, ItemSize),
                    ParentOrigin = ParentOrigin.TopLeft,
                    PivotPoint = PivotPoint.TopLeft,
                    PositionUsesPivotPoint = true,
                    Position = new Position(GetItemLocalX(i), ItemY, 0.0f),
                    BackgroundColor = itemColors[i % itemColors.Length],
                };
                content.Add(item);
                items[i] = item;

                TextLabel label = new TextLabel(i.ToString())
                {
                    Size = new Size(ItemSize, ItemSize),
                    ParentOrigin = ParentOrigin.Center,
                    PivotPoint = PivotPoint.Center,
                    PositionUsesPivotPoint = true,
                    TextColor = Color.White,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    PointSize = 18,
                };
                item.Add(label);
                itemLabels[i] = label;
            }
        }

        private void OnKeyEvent(object source, Window.KeyEventArgs e)
        {
            if (e.Key.State != Key.StateType.Down)
            {
                return;
            }

            string keyName = e.Key.KeyPressedName;
            if (keyName == "Escape" || keyName == "Back" || keyName == "XF86Back")
            {
                Deactivate();
                return;
            }

            if (keyName == "Left")
            {
                scrollOffset += MoveStep;
            }
            else if (keyName == "Right")
            {
                scrollOffset -= MoveStep;
            }
            else if (keyName == "0")
            {
                scrollOffset = 0.0f;
            }
            else if (keyName == "I" || keyName == "i")
            {
                appIgnoredCulling = !appIgnoredCulling;
            }
            else if (keyName == "C" || keyName == "c")
            {
                appIgnoredCulling = false;
                ClearIgnored();
            }
            else
            {
                return;
            }

            UpdateScene();
        }

        private void UpdateScene()
        {
            if (content == null)
            {
                return;
            }

            content.PositionX = scrollOffset;

            for (int i = 0; i < items.Length; i++)
            {
                if (items[i] != null)
                {
                    items[i].Ignored = appIgnoredCulling && IsFullyOutsideViewport(i);
                }
            }

            UpdateStatusText();
        }

        private void ClearIgnored()
        {
            if (items == null)
            {
                return;
            }

            for (int i = 0; i < items.Length; i++)
            {
                if (items[i] != null)
                {
                    items[i].Ignored = false;
                }
            }
        }

        private bool IsFullyOutsideViewport(int index)
        {
            float left = scrollOffset + GetItemLocalX(index);
            float right = left + ItemSize;
            return right <= 0.0f || left >= ViewportWidth;
        }

        private float GetItemLocalX(int index)
        {
            return ItemStartX + index * (ItemSize + ItemGap);
        }

        private void UpdateStatusText()
        {
            if (statusLabel == null || items == null)
            {
                return;
            }

            StringBuilder builder = new StringBuilder();
            builder.AppendLine($"offset={scrollOffset:0}  appIgnoredCulling={(appIgnoredCulling ? "ON" : "OFF")}  viewport=[0,{ViewportWidth:0}]");
            builder.AppendLine("idx expectedLeft expectedRight screenX ignored current world culled");

            for (int i = 0; i < items.Length; i++)
            {
                float expectedLeft = scrollOffset + GetItemLocalX(i);
                float expectedRight = expectedLeft + ItemSize;

                if (expectedRight < -ItemSize || expectedLeft > ViewportWidth + ItemSize)
                {
                    continue;
                }

                View item = items[i];
                Vector2 screenPosition = item.ScreenPosition;
                builder.AppendLine($"{i,2} {expectedLeft,8:0} {expectedRight,9:0} {screenPosition.X,7:0} {item.Ignored,7} {item.CurrentIgnored,7} {item.WorldIgnored,5} {item.Culled,6}");
            }

            statusLabel.Text = builder.ToString();
        }

        private void DisposeView<T>(ref T view) where T : View
        {
            if (view != null)
            {
                view.Unparent();
                view.Dispose();
                view = null;
            }
        }
    }
}
