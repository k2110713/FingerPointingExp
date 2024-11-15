using UnityEngine;
using System.Collections;
using System.IO.Ports;
using System.Threading;

public class SerialHandler : MonoBehaviour
{
    public delegate void SerialDataReceivedEventHandler(string message);
    public event SerialDataReceivedEventHandler OnDataReceived;

    public string portName = "COM4";
    public int baudRate = 9600;

    private SerialPort serialPort_;
    private Thread thread_;
    private bool isRunning_ = false;

    private string message_;
    private bool isNewMessageReceived_ = false;

    void Awake()
    {
        //Debug.Log("Initializing SerialHandler...");
        Open();
    }

    void Update()
    {
        if (isNewMessageReceived_ && isRunning_)
        {
            if (OnDataReceived != null)
            {
                //Debug.Log("Data received: " + message_);
                OnDataReceived(message_);
            }
        }
        isNewMessageReceived_ = false;
    }

    void OnDestroy()
    {
        //Debug.Log("Destroying SerialHandler...");
        Close();
    }

    private void Open()
    {
        try
        {
            serialPort_ = new SerialPort(portName, baudRate, Parity.None, 8, StopBits.One);
            serialPort_.Open();
            serialPort_.DtrEnable = true;
            serialPort_.RtsEnable = true;
            isRunning_ = true;

            //Debug.Log("Serial port opened: " + portName + " at " + baudRate + " baud");

            thread_ = new Thread(Read);
            thread_.Start();
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to open serial port: " + e.Message);
        }
    }

    public void Close()
    {
        isNewMessageReceived_ = false;
        isRunning_ = false;

        if (thread_ != null && thread_.IsAlive)
        {
            //Debug.Log("Stopping the read thread...");
            thread_.Abort();
            thread_.Join();
            //Debug.Log("Read thread stopped.");
        }

        if (serialPort_ != null && serialPort_.IsOpen)
        {
            //Debug.Log("Closing serial port...");
            serialPort_.Close();
            serialPort_.Dispose();
            //Debug.Log("Serial port closed.");
        }
    }

    private void Read()
    {
        while (isRunning_ && serialPort_ != null && serialPort_.IsOpen)
        {
            try
            {
                message_ = serialPort_.ReadLine();
                isNewMessageReceived_ = true;
                //Debug.Log("Read message: " + message_);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("Error reading from serial port: " + e.Message);
            }
        }
    }

    public void Write(string message)
    {
        try
        {
            if (serialPort_ != null && serialPort_.IsOpen)
            {
                serialPort_.Write(message);
                //Debug.Log("Written message: " + message);
            }
            else
            {
                Debug.LogWarning("Serial port not open. Cannot write message.");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("Error writing to serial port: " + e.Message);
        }
    }
}
