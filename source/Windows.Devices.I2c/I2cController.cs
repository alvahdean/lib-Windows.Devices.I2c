﻿//
// Copyright (c) 2017 The nanoFramework project contributors
// See LICENSE file in the project root for full license information.
//

using System;
using System.Collections;
using System.Runtime.CompilerServices;

namespace Windows.Devices.I2c
{
    /// <summary>
    /// Represents the I2C controller for the system.
    /// </summary>
	public sealed class I2cController
    {
        // this is used as the lock object 
        // a lock is required because multiple threads can access the I2cController
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private readonly object _syncLock;

        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private readonly int _controllerId;

        // backing field for DeviceCollection
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private Hashtable s_deviceCollection;

        /// <summary>
        /// Device collection associated with this <see cref="I2cController"/>.
        /// </summary>
        /// <remarks>
        /// This collection is for internal use only.
        /// </remarks>
        internal Hashtable DeviceCollection
        {
            get
            {
                if (s_deviceCollection == null)
                {
                    lock (_syncLock)
                    {
                        if (s_deviceCollection == null)
                        {
                            s_deviceCollection = new Hashtable();
                        }
                    }
                }

                return s_deviceCollection;
            }

            set
            {
                s_deviceCollection = value;
            }
        }

        internal I2cController(string controller)
        {
            // the I2C id is an ASCII string with the format 'I2Cn'
            // need to grab 'n' from the string and convert that to the integer value from the ASCII code (do this by subtracting 48 from the char value)
            _controllerId = controller[3] - '0';

            // check if this controller is already opened
            if (!I2cControllerManager.ControllersCollection.Contains(_controllerId))
            {
                _syncLock = new object();

                // call native init to allow HAL/PAL inits related with I2C hardware
                // this is also used to check if the requested ADC actually exists
                NativeInit();

                // add controller to collection, with the ID as key 
                // *** just the index number ***
                I2cControllerManager.ControllersCollection.Add(_controllerId, this);
            }
            else
            {
                // this controller already exists: throw an exception
                throw new ArgumentException();
            }
        }

        /// <summary>
        /// Gets the default I2C controller on the system.
        /// </summary>
        /// <returns>The default I2C controller on the system, or null if the system has no I2C controller.</returns>
        public static I2cController GetDefault()
        {
            string controllersAqs = GetDeviceSelector();
            string[] controllers = controllersAqs.Split(',');

            if (controllers.Length > 0)
            {
                // the I2C id is an ASCII string with the format 'I2Cn'
                // need to grab 'n' from the string and convert that to the integer value from the ASCII code (do this by subtracting 48 from the char value)
                var controllerId = controllers[0][3] - '0';

                if (I2cControllerManager.ControllersCollection.Contains(controllerId))
                {
                    // controller is already open
                    return (I2cController)I2cControllerManager.ControllersCollection[controllerId];
                }
                else
                {
                    // this controller is not in the collection, create it
                    return new I2cController(controllers[0]);
                }
            }

            // the system has no I2C controller 
            return null;
        }

        /// <summary>
        /// Gets the I2C device with the specified settings.
        /// </summary>
        /// <param name="settings"></param>
        /// <returns>The desired connection settings.</returns>
        public I2cDevice GetDevice(I2cConnectionSettings settings)
        {
            //TODO: fix return value. Should return an existing device (if any)
            return new I2cDevice(String.Empty, settings);
        }

        #region Native Calls

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void NativeInit();

        [MethodImpl(MethodImplOptions.InternalCall)]
        internal static extern string GetDeviceSelector();

        #endregion
    }
}
