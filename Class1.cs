using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.IO;
using System.Runtime.InteropServices;
using System.Globalization;
using System.Runtime.CompilerServices;
using MainInterface;

namespace VSCapture
{
    public sealed class DSerialPort : SerialPort
    {
        // Main Datex Record variables
        private datex_record_req_type request_ptr = new datex_record_req_type();
        private datex_record_wave_req_type wave_request_ptr = new datex_record_wave_req_type();
        public List<datex_record_type> RecordList = new List<datex_record_type>();
        public List<byte[]> FrameList = new List<byte[]>();
        private int DPortBufSize;
        public byte[] DPort_rxbuf;
        private datex_tx_type DPort_txbuf = new datex_tx_type();
        private datex_wave_tx_type DPort_wave_txbuf = new datex_wave_tx_type();

        private bool m_fstart = true;
        private bool m_storestart = false;
        private bool m_storeend = false;
        private bool m_bitshiftnext = false;
        private List<byte> m_bList = new List<byte>();
        private StringBuilder m_strBuilderWave = new StringBuilder();

        //private List<short> m_shECGList = new List<short>();
        public static ArrayList ECGList = new ArrayList();
        private static volatile DSerialPort DPort = null;

        public static DSerialPort getInstance
        {
            get
            {
                if (DPort == null)
                {
                    lock (typeof(DSerialPort))
                        if (DPort == null)
                        {
                            DPort = new DSerialPort();
                        }
                } return DPort;
            }
        }

        public DSerialPort()
        {
            DPort = this;
            //Workspace = GetWorkSpace();
            //CsvName = GetCsvName();

            DPortBufSize = 4096;
            DPort_rxbuf = new byte[DPortBufSize];
            DPort.PortName = "COM5"; //default Windows port
            DPort.BaudRate = 19200;
            DPort.Parity = Parity.Even;
            DPort.DataBits = 8;
            DPort.StopBits = StopBits.One;
            DPort.Handshake = Handshake.RequestToSend;

            // Set the read/write timeouts
            DPort.ReadTimeout = 600000;
            DPort.WriteTimeout = 600000;

            //ASCII Encoding in C# is only 7bit so
            DPort.Encoding = Encoding.GetEncoding("ISO-8859-1");
        }

        public void ClearReadBuffer()
        {
            //Clear the buffer
            for (int i = 0; i < DPortBufSize; i++)
            {
                DPort_rxbuf[i] = 0;
            }
        }

        public int ReadBuffer()
        {
            int bytesreadtotal = 0;

            try
            {
                int lenread = 0;

                do
                {
                    ClearReadBuffer();
                    lenread = DPort.Read(DPort_rxbuf, 0, DPortBufSize);

                    byte[] copyarray = new byte[lenread];

                    for (int i = 0; i < lenread; i++)
                    {
                        copyarray[i] = DPort_rxbuf[i];
                        CreateFrameListFromByte(copyarray[i]);
                    }

                    bytesreadtotal += lenread;
                    if (FrameList.Count > 0)
                    {
                        CreateRecordList();
                        ReadWaveSubRecords();

                        FrameList.RemoveRange(0, FrameList.Count);
                        RecordList.RemoveRange(0, RecordList.Count);
                    }

                }
                while (DPort.BytesToRead != 0);

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error opening/writing to serial port :: " + ex.Message, "Error!");
            }

            return bytesreadtotal;
        }

        public void CreateFrameListFromByte(byte b)
        {
            if (b == DataConstants.FRAMECHAR && m_fstart)
            {
                m_fstart = false;
                m_storestart = true;
            }
            else if (b == DataConstants.FRAMECHAR && (m_fstart == false))
            {
                m_fstart = true;
                m_storeend = true;
                m_storestart = false;
                if (b != DataConstants.FRAMECHAR) m_bList.Add(b);
            }

            if (m_storestart == true)
            {
                if (b == DataConstants.CTRLCHAR)
                    m_bitshiftnext = true;
                else
                {
                    if (m_bitshiftnext == true)
                    {
                        b |= DataConstants.BIT5;
                        m_bitshiftnext = false;
                        m_bList.Add(b);
                    }
                    else if (b != DataConstants.FRAMECHAR) m_bList.Add(b);
                }

            }
            else if (m_storeend == true)
            {
                int framelen = m_bList.Count();
                if (framelen != 0)
                {
                    byte[] bArray = new byte[framelen];
                    bArray = m_bList.ToArray();
                    //Calculate checksum
                    byte checksum = 0x00;
                    for (int j = 0; j < (framelen - 1); j++)
                    {
                        checksum += bArray[j];
                    }
                    if (checksum == bArray[framelen - 1])
                    {
                        FrameList.Add(bArray);
                    }
                    m_bList.Clear();
                    m_storeend = false;
                }
                else
                {
                    m_storestart = true;
                    m_storeend = false;
                    m_fstart = false;
                }
            }
        }

        public void CreateRecordList()
        {
            //Read record from Framelist();
            int recorddatasize = 0;
            byte[] fullrecord = new byte[1490];

            foreach (byte[] fArray in FrameList)
            {
                datex_record_type record_dtx = new datex_record_type();

                for (int i = 0; i < fullrecord.GetLength(0); i++)
                {
                    fullrecord[i] = 0x00;
                }

                recorddatasize = fArray.GetLength(0);

                for (int n = 0; n < (fArray.GetLength(0)) && recorddatasize < 1490; n++)
                {
                    fullrecord[n] = fArray[n];
                }

                GCHandle handle2 = GCHandle.Alloc(fullrecord, GCHandleType.Pinned);
                Marshal.PtrToStructure(handle2.AddrOfPinnedObject(), record_dtx);

                RecordList.Add(record_dtx);
                handle2.Free();
            }
        }

        public void ReadWaveSubRecords()
        {
            foreach (datex_record_type dx_record in RecordList)
            {
                short dxrecordmaintype = dx_record.hdr.r_maintype;

                if (dxrecordmaintype == DataConstants.DRI_MT_WAVE)
                {
                    short[] sroffArray = { dx_record.hdr.sr_offset1, dx_record.hdr.sr_offset2, dx_record.hdr.sr_offset3, dx_record.hdr.sr_offset4, dx_record.hdr.sr_offset5, dx_record.hdr.sr_offset6, dx_record.hdr.sr_offset7, dx_record.hdr.sr_offset8 };
                    byte[] srtypeArray = { dx_record.hdr.sr_type1, dx_record.hdr.sr_type2, dx_record.hdr.sr_type3, dx_record.hdr.sr_type4, dx_record.hdr.sr_type5, dx_record.hdr.sr_type6, dx_record.hdr.sr_type7, dx_record.hdr.sr_type8 };

                    uint unixtime = dx_record.hdr.r_time;
                    // Unix timestamp is seconds past epoch 
                    DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                    //dtDateTime = dtDateTime.AddSeconds(unixtime).ToLocalTime();
                    dtDateTime = dtDateTime.AddSeconds(unixtime);

                    string dtime = dtDateTime.ToLongTimeString();

                    //Read upto 8 subrecords
                    for (int i = 0; i < 8 && (srtypeArray[i] != DataConstants.DRI_EOL_SUBR_LIST); i++)
                    {
                        int offset = (int)sroffArray[i];
                        int nextoffset = 0;
                        //if (i==7) nextoffset = (1450 - offset);
                        if (i == 7) nextoffset = 1450;
                        else nextoffset = (int)sroffArray[i + 1];
                        //int nextoffset = (int)sroffArray [i + 1];

                        if (nextoffset <= offset || nextoffset > 1450) break;

                        int buflen = (nextoffset - offset - 6);

                        byte[] buffer = new byte[buflen];

                        for (int j = 0; j < buflen; j++)
                        {
                            buffer[j] = dx_record.data[6 + j + offset];
                        }

                        //Convert Byte array to 16 bit short values
                        for (int n = 0; n < buffer.Length; n += 2)
                        {
                            short wavedata = BitConverter.ToInt16(buffer, n);
                            //ShowWaveSubRecordData (wavedata);			
                            AddToWaveDataList(srtypeArray[i], wavedata);
                        }
                    }

                    ShowWaveSubRecordData(dtime);
                }
            }
        }

        public void AddToWaveDataList(byte waveDataType, short waveData)
        {
            if (waveDataType == DataConstants.DRI_WF_ECG1)
            {
                CommonVal.m_shECGList.Add(waveData);
            }
        }

        public void ShowWaveSubRecordData(string dTime)
        {
            foreach (var ecgValue in CommonVal.m_shECGList)
            {
                m_strBuilderWave.Append(dTime);
                m_strBuilderWave.Append(',');
                //SaveWaveDataLists ("ECG", waveValue, 0.01);
                const double decimalShift = 0.01;

                var s1 = ecgValue.ToString();
                ValidateAddWaveData(s1, decimalShift, false);

                var filename = string.Format("{0}.csv", CommonVal.DataType);
                var pathCsv = Path.Combine(CommonVal.Workspace, filename);

                ExportToWaveCsvFile(pathCsv);

                m_strBuilderWave.Remove(0, m_strBuilderWave.Length);
            }

            CommonVal.m_shECGList.Clear();
        }

        public bool ValidateAddWaveData(object value, double decimalShift, bool rounddata)
        {
            int val = Convert.ToInt32(value);
            double dval = (Convert.ToDouble(value, CultureInfo.InvariantCulture)) * decimalShift;
            if (rounddata) dval = Math.Round(dval);
            ECGList.Add(dval);

            string str = dval.ToString();

            if (val < DataConstants.DATA_INVALID_LIMIT)
            {
                str = "-";
                m_strBuilderWave.Append(str);
                m_strBuilderWave.Append(',');
                return false;
            }

            m_strBuilderWave.Append(str);
            m_strBuilderWave.Append(',');
            return true;
        }

        public void ExportToWaveCsvFile(string myFileName)
        {
            // Open file for reading. 
            StreamWriter wrStream = new StreamWriter(myFileName, true, Encoding.UTF8);

            wrStream.WriteLine(m_strBuilderWave);
            m_strBuilderWave.Remove(0, m_strBuilderWave.Length);
            // close file stream. 
            wrStream.Close();
        }

        //------------------------------------向监护仪发送指令------------------------------------
        public void WriteBuffer(byte[] txbuf)
        {
            byte[] framebyte = { DataConstants.CTRLCHAR, (DataConstants.FRAMECHAR & DataConstants.BIT5COMPL), 0 };
            byte[] ctrlbyte = { DataConstants.CTRLCHAR, (DataConstants.CTRLCHAR & DataConstants.BIT5COMPL), 0 };

            byte check_sum = 0x00;
            byte b1 = 0x00;
            byte b2 = 0x00;

            int txbuflen = (txbuf.GetLength(0) + 1);
            //Create write packet buffer
            byte[] temptxbuff = new byte[txbuflen];
            //Clear the buffer
            for (int j = 0; j < txbuflen; j++)
            {
                temptxbuff[j] = 0;
            }

            // Send start frame characters
            temptxbuff[0] = DataConstants.FRAMECHAR;

            int i = 1;

            foreach (byte b in txbuf)
            {
                switch (b)
                {
                    case DataConstants.FRAMECHAR:
                        temptxbuff[i] = framebyte[0];
                        temptxbuff[i + 1] = framebyte[1];
                        i += 2;
                        b1 += framebyte[0];
                        b1 += framebyte[1];
                        check_sum += b1;
                        break;
                    case DataConstants.CTRLCHAR:
                        temptxbuff[i] = ctrlbyte[0];
                        temptxbuff[i + 1] = ctrlbyte[1];
                        i += 2;
                        b2 += ctrlbyte[0];
                        b2 += ctrlbyte[1];
                        check_sum += b2;
                        break;
                    default:
                        temptxbuff[i] = b;
                        i++;
                        check_sum += b;
                        break;
                };
            }

            int buflen = i;
            byte[] finaltxbuff = new byte[buflen + 2];

            for (int j = 0; j < buflen; j++)
            {
                finaltxbuff[j] = temptxbuff[j];
            }

            // Send Checksum
            finaltxbuff[buflen] = check_sum;
            // Send stop frame characters
            finaltxbuff[buflen + 1] = DataConstants.FRAMECHAR;

            try
            {
                DPort.Write(finaltxbuff, 0, buflen + 2);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error opening/writing to serial port :: " + ex.Message, "Error!");
            }
        }

        public void RequestTransfer(byte Trtype, short Interval, byte DRIlevel)
        {
            //Set Record Header
            request_ptr.hdr.r_len = 49; //size of hdr + phdb type
            request_ptr.hdr.r_dri_level = DRIlevel;
            request_ptr.hdr.r_time = 0;
            request_ptr.hdr.r_maintype = DataConstants.DRI_MT_PHDB;

            request_ptr.hdr.sr_offset1 = 0;
            request_ptr.hdr.sr_type1 = 0; // Physiological data request
            request_ptr.hdr.sr_offset2 = 0;
            request_ptr.hdr.sr_type2 = 0xFF; // Last subrecord

            // Request transmission subrecord
            request_ptr.phdbr.phdb_rcrd_type = Trtype;
            request_ptr.phdbr.tx_interval = Interval;
            if (Interval != 0) request_ptr.phdbr.phdb_class_bf =
                  DataConstants.DRI_PHDBCL_REQ_BASIC_MASK | DataConstants.DRI_PHDBCL_REQ_EXT1_MASK |
                  DataConstants.DRI_PHDBCL_REQ_EXT2_MASK | DataConstants.DRI_PHDBCL_REQ_EXT3_MASK;
            else request_ptr.phdbr.phdb_class_bf = 0x0000;

            IntPtr uMemoryValue = IntPtr.Zero;

            try
            {
                uMemoryValue = Marshal.AllocHGlobal(49);
                Marshal.StructureToPtr(request_ptr, uMemoryValue, true);
                Marshal.PtrToStructure(uMemoryValue, DPort_txbuf);
                WriteBuffer(DPort_txbuf.data);
            }
            finally
            {
                if (uMemoryValue != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(uMemoryValue);
                }
            }
        }

        public void RequestWaveTransfer(short TrSignalType, byte DRIlevel)
        {
            //Set Record Header
            wave_request_ptr.hdr.r_len = 72; //size of hdr + wfreq type
            wave_request_ptr.hdr.r_dri_level = DRIlevel;
            wave_request_ptr.hdr.r_time = 0;
            wave_request_ptr.hdr.r_maintype = DataConstants.DRI_MT_WAVE;

            // The packet contains only one subrecord
            // 0 = Waveform data transmission request
            wave_request_ptr.hdr.sr_offset1 = 0;
            wave_request_ptr.hdr.sr_type1 = DataConstants.DRI_WF_CMD;
            wave_request_ptr.hdr.sr_offset2 = 0;
            wave_request_ptr.hdr.sr_type2 = DataConstants.DRI_EOL_SUBR_LIST; // Last subrecord

            // Request transmission subrecord
            wave_request_ptr.wfreq.req_type = TrSignalType;
            wave_request_ptr.wfreq.res = 0;

            if (TrSignalType == DataConstants.WF_REQ_CONT_STOP) //停止传输
            {
                wave_request_ptr.wfreq.type[0] = 0;
                wave_request_ptr.wfreq.type[1] = DataConstants.DRI_EOL_SUBR_LIST;
            }
            else //正常传输波形
            {
                wave_request_ptr.wfreq.type[0] = DataConstants.DRI_WF_ECG1;
                wave_request_ptr.wfreq.type[1] = DataConstants.DRI_WF_ECG2;
                for (int i = 2; i < 8; i++)
                    wave_request_ptr.wfreq.type[i] = DataConstants.DRI_EOL_SUBR_LIST;
            }

            //Ptr2Struct(72, wave_request_ptr, DPort_wave_txbuf);
            //Get pointer to structure in memory
            IntPtr uMemoryValue = IntPtr.Zero;

            try
            {
                uMemoryValue = Marshal.AllocHGlobal(72);
                Marshal.StructureToPtr(wave_request_ptr, uMemoryValue, true);
                Marshal.PtrToStructure(uMemoryValue, DPort_wave_txbuf);
                WriteBuffer(DPort_wave_txbuf.data);
            }
            finally
            {
                if (uMemoryValue != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(uMemoryValue);
                }
            }

        }

        public void StopWaveTransfer()
        {
            RequestWaveTransfer(DataConstants.WF_REQ_CONT_STOP, DataConstants.DRI_LEVEL_2005);
            RequestWaveTransfer(DataConstants.WF_REQ_CONT_STOP, DataConstants.DRI_LEVEL_2003);
            RequestWaveTransfer(DataConstants.WF_REQ_CONT_STOP, DataConstants.DRI_LEVEL_2001);
        }

        public void StopTransfer()
        {
            RequestTransfer(DataConstants.DRI_PH_DISPL, 0, DataConstants.DRI_LEVEL_2005);
            RequestTransfer(DataConstants.DRI_PH_DISPL, 0, DataConstants.DRI_LEVEL_2003);
            RequestTransfer(DataConstants.DRI_PH_DISPL, 0, DataConstants.DRI_LEVEL_2001);
        }
    }
}