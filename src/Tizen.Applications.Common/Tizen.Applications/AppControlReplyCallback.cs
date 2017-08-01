/*
 * Copyright (c) 2016 Samsung Electronics Co., Ltd All Rights Reserved
 *
 * Licensed under the Apache License, Version 2.0 (the License);
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an AS IS BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

namespace Tizen.Applications
{
    /// <summary>
    /// Reply callback for the launch request
    /// </summary>
    /// <param name="launchRequest">The AppControl of the launch request that has been sent</param>
    /// <param name="replyRequest">The AppControl in which the results of the callee are contained</param>
    /// <param name="result">The result of the launch request</param>
    public delegate void AppControlReplyCallback(AppControl launchRequest, AppControl replyRequest, AppControlReplyResult result);
}
