using System.Collections;
using System.Text;
using System.Threading;
using GalaevNetwork2.BitArrayRoutine;

namespace GalaevNetwork2
{
    public class FirstThread
    {
        private Semaphore _sendSemaphore;
        private Semaphore _receiveSemaphore;
        private BitArray _sendMessage;
        private BitArray _receivedMessage;
        private PostToSecondWT _post;

        public FirstThread(ref Semaphore sendSemaphore, ref Semaphore receiveSemaphore)
        {
            _sendSemaphore = sendSemaphore;
            _receiveSemaphore = receiveSemaphore;
        }

        public void FirstThreadMain(object obj)
        {
            _post = (PostToSecondWT)obj;

            // + Полудуплексный протокол (Confirmed by Zamanov https://yadi.sk/i/jBUw90I_pDuvcQ)
            // + Передача последовательности кадров
            // + Скользящее окно отсутствует
            // + Нумерация кадров отсутствует
            // + Запрос на соединение
            // + Запрос на разъединение
            // + Согласие на соединение
            // + Согласие на разъединение
            // + RR (Receiver Ready) - получатель готов принимать данные
            // + RNR (Receiver Not Ready) - получатель не готов принимать данные
            // + REJ (Reject) - получатель отверг принятые данные
            // + Добавить шумы (инвертировать один рандомный бит)

            ConsoleHelper.WriteToConsole("1 поток", "Начинаю работу.");

            if (!EstablishConnection()) return;
            ConsoleHelper.WriteToConsole("1 поток", "Подключен");
            
            SendDataChunk1();
            SendDataChunk2();

            if (!Disconnect()) return;
            ConsoleHelper.WriteToConsole("1 поток", "Отключен");

            ConsoleHelper.WriteToConsole("1 поток", "Завершаю работу.");
        }

        private void SendDataChunk1()
        {
            string dataString = "Алекс Городников ёбаный негр";
            var dataBytes = Encoding.UTF8.GetBytes(dataString);
            var dataBitArray = new BitArray(dataBytes);

            var inputBitArrays = dataBitArray.Split(C.MaxFrameDataSize);

            ConsoleHelper.WriteToConsole("1 поток", "Начало передачи");

            bool acceptedStartFrame = false;

            while (!acceptedStartFrame)
            {
                _sendMessage = BuildStartFrame().Build();
                _post(_sendMessage);
                _sendSemaphore.Release();
                _receiveSemaphore.WaitOne();

                var startResponseFrame = Frame.Parse(_receivedMessage);
                if (startResponseFrame.Control.ToByteArray()[0] == (byte)ResponseStatus.RR)
                {
                    acceptedStartFrame = true;
                }
                else
                {
                    ConsoleHelper.WriteToConsole("1 поток", "Кадр отвергнут");
                }
            }

            for (int i = 0; i < inputBitArrays.Count; i++)
            {
                bool acceptedDataFrame = false;
                while (!acceptedDataFrame)
                {
                    _sendMessage = BuildDataFrame(inputBitArrays[i]).Build();
                    _post(_sendMessage);
                    _sendSemaphore.Release();
                    ConsoleHelper.WriteToConsole("1 поток", $"Передан кадр {i}");
                    _receiveSemaphore.WaitOne();
                    
                    var dataResponseFrame = Frame.Parse(_receivedMessage);
                    if (dataResponseFrame.Control.ToByteArray()[0] == (byte)ResponseStatus.RR)
                    {
                        acceptedDataFrame = true;
                        ConsoleHelper.WriteToConsole("1 поток", "Кадр отвергнут");
                    }
                }
            }

            ConsoleHelper.WriteToConsole("1 поток", "Конец передачи");
            
            bool acceptedEndFrame = false;

            while (!acceptedEndFrame)
            {
                _sendMessage = BuildEndFrame().Build();
                _post(_sendMessage);
                _sendSemaphore.Release();
                _receiveSemaphore.WaitOne();

                var endResponseFrame = Frame.Parse(_receivedMessage);
                if (endResponseFrame.Control.ToByteArray()[0] == (byte)ResponseStatus.RR)
                {
                    acceptedEndFrame = true;
                }
                else
                {
                    ConsoleHelper.WriteToConsole("1 поток", "Кадр отвергнут");
                }
            }
        }

        private void SendDataChunk2()
        {
            string dataString = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.";
            var dataBytes = Encoding.UTF8.GetBytes(dataString);
            var dataBitArray = new BitArray(dataBytes);

            var inputBitArrays = dataBitArray.Split(C.MaxFrameDataSize);

            ConsoleHelper.WriteToConsole("1 поток", "Начало передачи");

            bool acceptedStartFrame = false;

            while (!acceptedStartFrame)
            {
                _sendMessage = BuildStartFrame().Build();
                _post(_sendMessage);
                _sendSemaphore.Release();
                _receiveSemaphore.WaitOne();

                var startResponseFrame = Frame.Parse(_receivedMessage);
                if (startResponseFrame.Control.ToByteArray()[0] == (byte)ResponseStatus.RR)
                {
                    acceptedStartFrame = true;
                }
                else
                {
                    ConsoleHelper.WriteToConsole("1 поток", "Кадр отвергнут");
                }
            }

            for (int i = 0; i < inputBitArrays.Count; i++)
            {
                bool acceptedDataFrame = false;
                while (!acceptedDataFrame)
                {
                    _sendMessage = BuildDataFrame(inputBitArrays[i]).Build();
                    _post(_sendMessage);
                    _sendSemaphore.Release();
                    ConsoleHelper.WriteToConsole("1 поток", $"Передан кадр {i}");
                    _receiveSemaphore.WaitOne();
                    
                    var dataResponseFrame = Frame.Parse(_receivedMessage);
                    if (dataResponseFrame.Control.ToByteArray()[0] == (byte)ResponseStatus.RR)
                    {
                        acceptedDataFrame = true;
                    }
                    else
                    {
                        ConsoleHelper.WriteToConsole("1 поток", "Кадр отвергнут");
                    }
                }
            }

            ConsoleHelper.WriteToConsole("1 поток", "Конец передачи");
            
            bool acceptedEndFrame = false;

            while (!acceptedEndFrame)
            {
                _sendMessage = BuildEndFrame().Build();
                _post(_sendMessage);
                _sendSemaphore.Release();
                _receiveSemaphore.WaitOne();

                var endResponseFrame = Frame.Parse(_receivedMessage);
                if (endResponseFrame.Control.ToByteArray()[0] == (byte)ResponseStatus.RR)
                {
                    acceptedEndFrame = true;
                }
                else
                {
                    ConsoleHelper.WriteToConsole("1 поток", "Кадр отвергнут");
                }
            }
        }

        private bool EstablishConnection()
        {
            bool connected = false;
            int tries = 0;
            while (!connected && tries < 3)
            {
                tries++;
                
                _sendMessage = BuildConnectFrame().Build();
                _post(_sendMessage);
                _sendSemaphore.Release();

                if (!WaitForResponseWithTimeout())
                {
                    ConsoleHelper.WriteToConsole("1 поток", "Соединение не установлено");
                    continue;
                }

                var connectResponseFrame = Frame.Parse(_receivedMessage);

                if (connectResponseFrame.Control.ToByteArray()[0] == (byte)ResponseStatus.RR)
                {
                    connected = true;
                }
                else
                {
                    ConsoleHelper.WriteToConsole("1 поток", $"Подключение отвергнуто {tries} раз");
                }
            }

            return tries != 3;
        }

        private bool Disconnect()
        {
            bool disconnected = false;
            int tries = 0;
            while (!disconnected && tries < 3)
            {
                tries++;
                
                _sendMessage = BuildDisconnectFrame().Build();
                _post(_sendMessage);
                _sendSemaphore.Release();

                if (!WaitForResponseWithTimeout())
                {
                    ConsoleHelper.WriteToConsole("1 поток", "Ответ на отключение не получен");
                    continue;
                }

                var disconnectResponseFrame = Frame.Parse(_receivedMessage);

                if (disconnectResponseFrame.Control.ToByteArray()[0] == (byte)ResponseStatus.RR)
                {
                    disconnected = true;
                }
                else
                {
                    ConsoleHelper.WriteToConsole("1 поток", $"Отключение отвергнуто {tries} раз");
                }
            }

            return tries != 3;
        }

        private bool WaitForResponseWithTimeout()
        {
            int tries = 0;
            while (!_receiveSemaphore.WaitOne(500) && tries < 3)
            {
                // Timeout
                ConsoleHelper.WriteToConsole("1 поток", $"Таймаут получения {++tries} раз");
            }

            return tries != 3;
        }

        private Frame BuildConnectFrame()
        {
            var frame = new Frame(new BitArray(C.ControlSize), new BitArray(0));

            var bitArrayWriter = new BitArrayWriter(frame.Control);
            bitArrayWriter.Write(new BitArray(new[] {(byte)RequestType.Connect, (byte)0}));

            return frame;
        }

        private Frame BuildStartFrame()
        {
            var frame = new Frame(new BitArray(C.ControlSize), new BitArray(0));

            var bitArrayWriter = new BitArrayWriter(frame.Control);
            bitArrayWriter.Write(new BitArray(new[] {(byte)RequestType.Start, (byte)0}));

            return frame;
        }

        private static Frame BuildDataFrame(BitArray data)
        {
            var frame = new Frame(new BitArray(C.ControlSize), data);

            var bitArrayWriter = new BitArrayWriter(frame.Control);
            bitArrayWriter.Write(new BitArray(new[] {(byte)RequestType.Data, (byte)0}));

            return frame;
        }

        private static Frame BuildEndFrame()
        {
            var frame = new Frame(new BitArray(C.ControlSize), new BitArray(0));

            var bitArrayWriter = new BitArrayWriter(frame.Control);
            bitArrayWriter.Write(new BitArray(new[] {(byte)RequestType.End, (byte)0}));

            return frame;
        }

        private Frame BuildDisconnectFrame()
        {
            var frame = new Frame(new BitArray(C.ControlSize), new BitArray(0));

            var bitArrayWriter = new BitArrayWriter(frame.Control);
            bitArrayWriter.Write(new BitArray(new[] {(byte)RequestType.Disconnect, (byte)0}));

            return frame;
        }

        public void ReceiveData(BitArray array)
        {
            _receivedMessage = array;
        }
    }
}