//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//
//
// Use of this sample source code is subject to the terms of the Microsoft
// license agreement under which you licensed this sample source code. If
// you did not accept the terms of the license agreement, you are not
// authorized to use this sample source code. For the terms of the license,
// please see the license agreement between you and Microsoft or, if applicable,
// see the LICENSE.RTF on your install media or the root of your tools installation.
// THE SAMPLE SOURCE CODE IS PROVIDED "AS IS", WITH NO WARRANTIES OR INDEMNITIES.
//
#region Using directives

using System;

#endregion

namespace Microsoft.WindowsMobile.Samples.Location
{
    /// <summary>
    /// Event args used for DeviceStateChanged event.
    /// </summary>
    public class DeviceStateChangedEventArgs: EventArgs
    {
        public DeviceStateChangedEventArgs(GpsDeviceState deviceState)
        {
            this.deviceState = deviceState;
        }

        /// <summary>
        /// Gets the new device state when the GPS reports a new device state.
        /// </summary>
        public GpsDeviceState DeviceState
        {
            get 
            {
                return deviceState;
            }
        }

        private GpsDeviceState deviceState;
    }
}
