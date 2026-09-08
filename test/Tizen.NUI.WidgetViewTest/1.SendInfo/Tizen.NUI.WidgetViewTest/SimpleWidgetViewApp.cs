/*
 * Copyright (c) 2022 Samsung Electronics Co., Ltd.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 */
using System;
using System.Collections.Generic;
using Tizen.NUI;
using Tizen.NUI.BaseComponents;
using Tizen.Applications;
using Tizen.Applications.Messages;

namespace WidgetApplicationTemplate
{
    class Program : NUIApplication
    {
        string widgetAppId = "Tizen.NUI.WidgetTest";
        string rcvPort = "my_widget_port";

        protected override void OnCreate()
        {
            base.OnCreate();
            Initialize();
        }
        void Initialize()
        {
            Window window = GetDefaultWindow();

            window.KeyEvent += OnKeyEvent;
            window.TouchEvent += OnTouchEvent;
            
            window.BackgroundColor = Color.White;
            rootView = new View { BackgroundColor = Color.White };
            window.GetDefaultLayer().Add(rootView);
            titleLabel = CreateLabel("Widget Sample - NUI Provider\nTouch widget: color/count\n1: resize | Provider fixed for viewer comparison", Color.Black);
            providerLabel = CreateLabel("Active: NUI  |  Fixed provider", Color.White);
            providerLabel.BackgroundColor = new Color(0.50f, 0.28f, 0.72f, 1.0f);
            widgetArea = new View { BackgroundColor = new Color(0.96f, 0.97f, 0.99f, 1.0f) };
            statusLabel = CreateLabel("Creating widgets from Tizen.NUI.WidgetTest", Color.Black);
            rootView.Add(titleLabel);
            rootView.Add(providerLabel);
            rootView.Add(widgetArea);
            rootView.Add(statusLabel);
            ApplyResponsiveLayout();
            window.Resized += OnWindowResized;

            Bundle bundle = new Bundle();
            bundle.AddItem("COUNT", "1");
            String encodedBundle = bundle.Encode();

            // Compute the initial surface size before AddWidget.
            mWidgetView = WidgetViewManager.Instance.AddWidget("class1@Tizen.NUI.WidgetTest", encodedBundle, widgetWidth, widgetHeight, 0.0f);
            widgetArea.Add(mWidgetView);

            //mWidgetView.WidgetContentUpdated += OnWidgetContentUpdatedCB;

            mWidgetView2 = WidgetViewManager.Instance.AddWidget("class2@Tizen.NUI.WidgetTest", encodedBundle, widgetWidth, widgetHeight, 0.0f);
            widgetArea.Add(mWidgetView2);
            UpdateWidgetBounds();
            bundle.Dispose();

            // Send message using port
            _msgPort = new MessagePort(rcvPort, false);
            Tizen.Log.Info("NUI", "MessagePort Create: " + _msgPort.PortName + "Trusted: " + _msgPort.Trusted);
            _msgPort.Listen();
        }
        private static TextLabel CreateLabel(string text, Color color)
        {
            return new TextLabel(text)
            {
                TextColor = color,
                MultiLine = true,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
        }

        private static void SetBounds(View view, float x, float y, float width, float height)
        {
            view.ParentOrigin = ParentOrigin.TopLeft;
            view.PivotPoint = PivotPoint.TopLeft;
            view.PositionUsesPivotPoint = true;
            view.Position = new Position(x, y);
            view.SizeWidth = width;
            view.SizeHeight = height;
        }

        private static void SetCornerRadius(View view, float radius)
        {
            view.CornerRadiusPolicy = VisualTransformPolicyType.Absolute;
            view.CornerRadius = new Vector4(radius, radius, radius, radius);
        }

        private void OnWindowResized(object sender, Window.ResizedEventArgs e)
        {
            ApplyResponsiveLayout();
        }

        private void ApplyResponsiveLayout()
        {
            // Match dali-ui samples/widget-viewer/ApplyResponsiveLayout.
            var size = GetDefaultWindow().Size;
            float scale = Math.Max(0.85f, Math.Min(2.0f, Math.Min(size.Width / 1280.0f, size.Height / 800.0f)));
            float padding = 32.0f * scale;
            float spacing = 14.0f * scale;
            widgetGap = 48.0f * scale;
            contentWidth = Math.Max(1.0f, size.Width - 2.0f * padding);
            widgetAreaHeight = Math.Max(240.0f, size.Height - 2.0f * padding -
                (86.0f + 52.0f + 48.0f) * scale - 3.0f * spacing);
            widgetCornerRadius = 40.0f * scale;

            SetBounds(rootView, 0, 0, size.Width, size.Height);
            float y = padding;
            SetBounds(titleLabel, padding, y, contentWidth, 86.0f * scale);
            titleLabel.PixelSize = 14.0f * scale;
            y += 86.0f * scale + spacing;
            float providerWidth = Math.Max(480.0f * scale, Math.Min(1200.0f, contentWidth * 0.58f));
            SetBounds(providerLabel, (size.Width - providerWidth) * 0.5f, y, providerWidth, 52.0f * scale);
            providerLabel.PixelSize = 14.0f * scale;
            SetCornerRadius(providerLabel, 12.0f * scale);
            y += 52.0f * scale + spacing;
            SetBounds(widgetArea, padding, y, contentWidth, widgetAreaHeight);
            SetCornerRadius(widgetArea, 18.0f * scale);
            y += widgetAreaHeight + spacing;
            SetBounds(statusLabel, padding, y, contentWidth, 48.0f * scale);
            statusLabel.PixelSize = 12.0f * scale;

            float slotWidth = Math.Max(1.0f, (contentWidth - widgetGap) * 0.5f);
            float idealSize = Math.Min(slotWidth * 0.72f, widgetAreaHeight * 0.78f);
            int previousBase = baseWidgetSize;
            baseWidgetSize = Math.Max(20, (int)(Math.Max(300.0f, Math.Min(640.0f, idealSize)) / 20.0f) * 20);
            maximumWidgetSize = Math.Max(baseWidgetSize,
                (int)Math.Min(760.0f, Math.Min(slotWidth * 0.92f, widgetAreaHeight * 0.90f)));
            widgetResizeStep = Math.Min(Math.Max(60, baseWidgetSize / 5), maximumWidgetSize - baseWidgetSize);
            if (mWidgetView == null || (widgetWidth == previousBase && widgetHeight == previousBase))
            {
                widgetWidth = baseWidgetSize;
                widgetHeight = baseWidgetSize;
            }
            else
            {
                widgetWidth = Math.Min(widgetWidth, maximumWidgetSize);
                widgetHeight = Math.Min(widgetHeight, maximumWidgetSize);
            }
            UpdateWidgetBounds();
        }

        private void UpdateWidgetBounds()
        {
            float slotWidth = Math.Max(1.0f, (contentWidth - widgetGap) * 0.5f);
            if (mWidgetView != null)
            {
                SetCornerRadius(mWidgetView, widgetCornerRadius);
                SetBounds(mWidgetView, Math.Max(0, (slotWidth - widgetWidth) * 0.5f),
                    Math.Max(0, (widgetAreaHeight - widgetHeight) * 0.5f), widgetWidth, widgetHeight);
            }
            if (mWidgetView2 != null)
            {
                SetCornerRadius(mWidgetView2, widgetCornerRadius);
                SetBounds(mWidgetView2, slotWidth + widgetGap + Math.Max(0, (slotWidth - baseWidgetSize) * 0.5f),
                    Math.Max(0, (widgetAreaHeight - baseWidgetSize) * 0.5f), baseWidgetSize, baseWidgetSize);
            }
        }

        public void OnKeyEvent(object sender, Window.KeyEventArgs e)
        {
            if (e.Key.State == Key.StateType.Down )
            {
                Tizen.Log.Info("NUI", "OnKeyEvent(View-Window) : " + e.Key.KeyPressedName + "\n");
                if (e.Key.KeyPressedName == "1")
                {
                    widgetWidth += widgetResizeStep;
                    widgetHeight += widgetResizeStep;
                    if (widgetWidth > maximumWidgetSize || widgetHeight > maximumWidgetSize)
                    {
                        widgetWidth = baseWidgetSize;
                        widgetHeight = baseWidgetSize;
                    }
                    UpdateWidgetBounds();

                    // Send the WidgetView's width to Widget
                    var msg = new Bundle();
                    msg.AddItem("message", "WidgetView's message >> width:" + widgetWidth);
                    _msgPort.Send(msg, widgetAppId, rcvPort);
                }

            }
        }
        private void OnTouchEvent(object source, Window.TouchEventArgs e)
        {
        }

/*
        private void OnWidgetContentUpdatedCB(object sender, WidgetView.WidgetViewEventArgs e)
        {
            String encodedBundle = e.WidgetView.ContentInfo;
            Tizen.Log.Info("NUI", "tscholb : OnWidgetContentUpdatedCB : " + encodedBundle + "\n");
            Bundle bundle = Bundle.Decode(encodedBundle);
            string outString;
            if (bundle.TryGetItem("COUNT", out outString))
            {
                Tizen.Log.Info("NUI", "OnWidgetContentUpdatedCB(2) : " + outString + "\n");
            }

        }
*/

        static void Main(string[] args)
        {
            var app = new Program();
            app.Run(args);
        }

        private static MessagePort _msgPort;
        private View rootView;
        private View widgetArea;
        private TextLabel titleLabel;
        private TextLabel providerLabel;
        private TextLabel statusLabel;
        private float contentWidth;
        private float widgetAreaHeight;
        private float widgetGap;
        private float widgetCornerRadius;
        private int baseWidgetSize;
        private int maximumWidgetSize;
        private int widgetResizeStep;
        WidgetView mWidgetView;
        WidgetView mWidgetView2;
        int widgetWidth;
        int widgetHeight;
    }
}
