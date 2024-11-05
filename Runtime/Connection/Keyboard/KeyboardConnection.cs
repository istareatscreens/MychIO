using System;
using System.Diagnostics;
using System.Threading.Tasks;
using MychIO.Device;
using UnityEngine;

namespace MychIO.Connection.Keyboard
{
    public class KeyboardConnection : Connection
    {
        private bool _isReading = false;

        public KeyboardConnection(IDevice device, IConnectionProperties connectionProperties, IOManager manager)
            : base(device, connectionProperties, manager)
        {
        }

        public override Task Connect()
        {
            Read();
            return Task.CompletedTask;
        }

        public override Task Disconnect()
        {
            StopReading();
            return Task.CompletedTask;
        }

        public override bool IsConnected()
        {
            return _isReading;
        }

        public override async Task Write(byte[] bytes)
        {
            // Keyboard input typically doesn't require writing data; left as a no-op
            await Task.CompletedTask;
        }

        public override bool CanConnect(IConnection connectionProperties)
        {
            return true;
        }

        public override bool IsReading()
        {
            return _isReading;
        }

        public override void StopReading()
        {
            _isReading = false;
            IOManager.CoroutineRunner.StopCoroutine(CaptureInput());
        }

        public override void Read()
        {
            _isReading = true;
            IOManager.CoroutineRunner.StartCoroutine(CaptureInput());
        }

        private System.Collections.IEnumerator CaptureInput()
        {
            while (_isReading)
            {
                // Capture any character input from the user
                string input = Input.inputString;

                if (!string.IsNullOrEmpty(input))
                {
                    _device.ReadData(input);
                }

                yield return null; // Wait for the next frame
            }
        }


    }
}