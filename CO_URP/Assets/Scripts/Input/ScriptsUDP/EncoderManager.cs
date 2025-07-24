using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using UnityEngine;
using System;
using ManualRead;
using Peak.Can.Basic;
// using ManualWrite;
using System.IO;
using MathNet;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

// Type alias for a PCAN-Basic channel handle 
using TPCANHandle = System.UInt16;
// Type alias for a CAN-FD bitrate string  
using TPCANBitrateFD = System.String;
// Type alias for a microseconds timestamp  
using TPCANTimestampFD = System.UInt64;

public class EncoderManager : MonoBehaviour
{
    /// <summary>
    /// definition for games 
    /// </summary>
    public static EncoderManager instance;
    // Start is called before the first frame update

    //[SerializeField, Range(-180f, 180f)]
    public float x_rotation = 0f;
    //[SerializeField, Range(-180f, 180f)]
    public float z_rotation = 0f;

    public float y_rotation = 0f;

    #region Defines
    /// <summary>
    /// Sets the PCANHandle (Hardware Channel)
    /// </summary>
    const TPCANHandle PcanHandle = PCANBasic.PCAN_USBBUS1;
    /// <summary>
    /// Sets the desired connection mode (CAN = false / CAN-FD = true)
    /// </summary>
    const bool IsFD = false;
    /// <summary>
    /// Sets the bitrate for normal CAN devices
    /// </summary>
    const TPCANBaudrate Bitrate = TPCANBaudrate.PCAN_BAUD_1M;

    /// <summary>
    /// Sets the bitrate for CAN FD devices.  
    /// Example - Bitrate Nom: 1Mbit/s Data: 2Mbit/s: 
    ///   "f_clock_mhz=20, nom_brp=5, nom_tseg1=2, nom_tseg2=1, nom_sjw=1, data_brp=2, data_tseg1=3, data_tseg2=1, data_sjw=1"
    /// </summary> 
    const TPCANBitrateFD BitrateFD = "f_clock_mhz=20, nom_brp=5, nom_tseg1=2, nom_tseg2=1, nom_sjw=1, data_brp=2, data_tseg1=3, data_tseg2=1, data_sjw=1";
    #endregion

    #region Members
    /// <summary>
    /// Shows if DLL was found
    /// </summary>
    private bool m_DLLFound;
    #endregion

    public System.UInt32 angle;

    public static double angle_to_radian = Math.PI / 180.0;
    public static double radian_to_angle = 180.0 / Math.PI;

    public static double SPM_R = 70;
    public static double SPM_R_1 = SPM_R * Math.Sqrt(3);
    public static double SPM_R_2 = SPM_R * 2;
    public static double beta = 60.0 * angle_to_radian;
    public static double alpha_1 = 45 * angle_to_radian;
    public static double alpha_2 = 45 * angle_to_radian;
    public static double alpha_3 = 2 * Math.Asin(Math.Sin(beta) * Math.Cos(Math.PI / 6));

    /// <summary>
    /// calulation hyperparameters
    /// </summary>
    public static double FE_angle_right = 0.0;
    public static double FE_angle_left = 0.0;
    public static double RU_angle = 0.0;
    public static double PS_angle = 0.0;

    public double theta_1_t = -54.0;
    public double theta_2_t = -54.0;
    public double theta_3_t = -54.0;
    public double theta_4_t = -54.0;
    public double theta_5_t = -54.0;
    public double theta_6_t = -54.0;

    public static double theta_1_zero = -54.0;
    public static double theta_2_zero = -54.0;
    public static double theta_3_zero = -54.0;
    public static double theta_4_zero = -54.0;
    public static double theta_5_zero = -54.0;
    public static double theta_6_zero = -54.0;

    public static byte can_id_1 = 0x01;
    public static byte can_id_2 = 0x02;
    public static byte can_id_3 = 0x03;
    public static byte can_id_4 = 0x04;
    public static byte can_id_5 = 0x05;
    public static byte can_id_6 = 0x06;

    public DenseVector theta_t_zero_left = new DenseVector(3);
    public DenseVector theta_t_list_ori_left = new DenseVector(3);
    public DenseVector theta_t_list_left = new DenseVector(3);
    public DenseVector theta_t_zero_right = new DenseVector(3);
    public DenseVector theta_t_list_ori_right = new DenseVector(3);
    public DenseVector theta_t_list_right = new DenseVector(3);
    
    public DenseVector encoder_increment_data = new DenseVector(3);    //  编码器超限问题，设置参数 
    public int encoder_min = 0;    // 编码器最小值
    public int encoder_max = 1024*16;  // 编码器最大值
    public int encoder_flag = 0;      // 初始化编码器是否开始读数据，是：1， 否：0
    public DenseVector  encoder_last = new DenseVector(3);   // 编码器上一个读取的数据
    public int[] encoder_cirls = new int[3];   

    public DenseVector trans_ratio_list = new DenseVector(3);
    public DenseVector trans_ratio_list_left = new DenseVector(3);
    public DenseVector trans_ratio_list_right = new DenseVector(3);

    public DenseVector x_list_left = new DenseVector(9);
    public DenseVector x_list_t_left = new DenseVector(9);
    public DenseVector ee_n_left = new DenseVector(3);
    public DenseVector euler_angle_left = new DenseVector(3);
    public DenseVector rotate_angle_left = new DenseVector(3);

    public DenseVector x_list_right = new DenseVector(9);
    public DenseVector x_list_t_right = new DenseVector(9);
    public DenseVector ee_n_right = new DenseVector(3);
    public DenseVector euler_angle_right = new DenseVector(3);
    public DenseVector rotate_angle_right = new DenseVector(3);

    //public DensVector encoder_rotate_vec = new DensVector(6); 
    public DenseVector ori_n = new DenseVector(3);

    public DenseVector theta_initial_passive = new DenseVector(3);

    public TPCANStatus stsResult;

    /// <summary>
    /// save data 
    /// </summary>
    ///
    const bool SaveFile = false;
    public StreamWriter ee_pose_unity;
    public StreamWriter encoder_data_unity;
    public static string subject_name = "_Shike";
    public static int index = 1; // revise 
    public long start_time;
    public long current_time;
    /// <summary>
    /// Read initial value
    /// </summary>
    #region Read initial value
    public int equipType;
    public double initvalue1;
    public double initvalue2;
    public double initvalue3;
    public double tranRatio1;
    public double tranRatio2;
    public double tranRatio3;
    public int precision;
    #endregion
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(this);
        }
    }
    void Start()
    {
        //Initial value initialization
        initvalue1 = ReadConfig.instance.initvalue1;
        initvalue2 = ReadConfig.instance.initvalue2;
        initvalue3 = ReadConfig.instance.initvalue3;
        equipType= ReadConfig.instance.equipType;
        tranRatio1 = ReadConfig.instance.tranRatio1;
        tranRatio2 = ReadConfig.instance.tranRatio2;
        tranRatio3 = ReadConfig.instance.tranRatio3;
        precision = ReadConfig.instance.precision;
        // ��ʼ�� ///
        ManualWriteSample();

        if (SaveFile)
        {
            start_time = DateTime.Now.Ticks;
            ee_pose_unity = new StreamWriter("subject_data\\new_ee_pose_unity_subject_" + index.ToString() + ".txt");
            encoder_data_unity = new StreamWriter("subject_data\\new_encoder_data_unity_subject_" + index.ToString() + ".txt");
        }

        //Debug.Log("Theta_t_list_right :" + theta_t_list_right[0].ToString() + " ," + theta_t_list_right[1].ToString() + ", " + theta_t_list_right[2].ToString());
        /*if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            //Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }*/
    }


    void OnDisable()
    {
        reset();

        if (SaveFile)
        {
            ee_pose_unity.Close();
            encoder_data_unity.Close();
        }

        Destroy(this);
    }

    // Update is called once per frame 
    void Update()
    {
        // read one step values
        ReadEncoderStep();

        //rotate_angle_right = result_vec.right;           
        //rotate_angle_left = result_vec.left;   

        //x_rotation = -1*(float)rotate_angle_right[1];   
        //z_rotation = (float)rotate_angle_right[0];   
        //y_rotation = (float)FE_angle_right;  

       
        x_rotation = (float)rotate_angle_right[0];
        if (equipType == 1) 
        {
            z_rotation = (float)rotate_angle_right[1];
        }else
        {
            z_rotation = -1*(float)rotate_angle_right[1];
        }     
        y_rotation = (float)FE_angle_right;

        //y_rotation = (float)FE_angle_left; 
        
        transform.rotation = Quaternion.Euler(x_rotation, y_rotation, z_rotation);
        //Debug.Log("x_rotation :" + (float)x_rotation + " y_rotation :" + (float)y_rotation + "  z_rotation :" + (float)z_rotation);
       // Debug.Log("x_rotation :" + (float)theta_t_list_right[0] + " y_rotation :" + (float)theta_t_list_right[1] + "  z_rotation :" + (float)theta_t_list_right[2]);

        //Debug.Log("encoder init :" +  theta_t_zero_right[0] + ", " +  theta_t_zero_right[1] +", " +  theta_t_zero_right[2]);
        //Debug.Log("encoder current :" +  theta_t_list_ori_right[0] + ", " +  theta_t_list_ori_right[1] +", " +  theta_t_list_ori_right[2]);
        //Debug.Log("encoder increment:" +  encoder_increment_data[0] + ", " +  encoder_increment_data[1] +", " +  encoder_increment_data[2]);
        //Debug.Log("encoder cirls:" +  encoder_cirls[0] + ", " +  encoder_cirls[1] +", " +  encoder_cirls[2]);*/
      
        //Debug.Log("a :" + (float)rotate_angle_right[0] + " b :" + (float)FE_angle_right + "  c :" + (float)rotate_angle_right[1]);
    }

    ////////////////////////// defined functions //////////////////////////////////
    /// <summary> 
    /// Starts the PCANBasic Sample 
    /// </summary> 
    public void ManualWriteSample()
    {
        // Shows information about this sample   
        ShowConfigurationHelp();

        // Shows the current parameters configuration    
        ShowCurrentConfiguration();

        // Checks if PCANBasic.dll is available, if not, the program terminates
        m_DLLFound = CheckForLibrary();
        if (!m_DLLFound)
            return;

        TPCANStatus stsResult;
        // Initialization of the selected channel
        if (IsFD)
            stsResult = PCANBasic.InitializeFD(PcanHandle, BitrateFD);
        else
            stsResult = PCANBasic.Initialize(PcanHandle, Bitrate);

        if (stsResult != TPCANStatus.PCAN_ERROR_OK)
        {
            Console.WriteLine("Can not initialize. Please check the defines in the code.");
            ShowStatus(stsResult);
            Console.WriteLine();
            Console.WriteLine("Press any key to close");
            Console.Read();
            return;
        }

        ////////////////////////////////////////////////////////////////////////  
        //////////////////////////// Initilization /////////////////////////////  
        ////////////////////////////////////////////////////////////////////////  
        ori_n.SetValues(new[] { 0.0, 0.0, -70.0 });

        x_list_right.SetValues(new[] { 0.5, -0.70710678, -0.5, 0.36237243, 0.78656609, -0.5, -0.86237244, -0.07945932, -0.5 });
        x_list_left.SetValues(new[] { 0.5, -0.70710678, -0.5, 0.36237243, 0.78656609, -0.5, -0.86237244, -0.07945932, -0.5 });

/*        //trans_ratio_list.SetValues(new[] { 1.0 / 3.0, -1.0 / 3.0, -1.0 / 3.0 });
        trans_ratio_list_right = 360.0 / 4096.0 * trans_ratio_list;
        //trans_ratio_list_left = 360.0 / 1024.0 * trans_ratio_list;  
        trans_ratio_list_left = 360.0 / 4096.0 * trans_ratio_list;*/
        trans_ratio_list.SetValues(new[] { tranRatio1, tranRatio2, tranRatio3 });
        trans_ratio_list_right = 360.0 / precision * trans_ratio_list; 
        trans_ratio_list_left = 360.0 / precision * trans_ratio_list;

        /// set zero position 
        /// passive version
        ////theta_t_zero_right = ReadZeroValue();  
        //theta_t_zero_right.SetValues(new[] { 65906.0, 9968.0, 10430.0 });  /// reset manully  

        ////theta_t_zero_left = ReadZeroValue(); 
        //theta_t_zero_left.SetValues(new[] { 19580.0, 21122.0, 2116.0 });  /// reset manully  

        // set acive zero position nus version 
        //theta_t_zero_left.SetValues(new[] { 13922.0, 15193.0,14828.0 });
        // set acive zero position domestic version 
        //theta_t_zero_left.SetValues(new[] { 14051.0,13883.0,13580.0 });
        theta_t_zero_left.SetValues(new[] { initvalue1, initvalue2, initvalue3 });
        //theta_t_zero_left.SetValues(new[] { 51938.0, 70788.0, 15126.0 });
        // set zero position nus version 
        //theta_t_zero_right.SetValues(new[] { 13922.0, 15193.0,14828.0 });
        // set acive zero position domestic version 
        //theta_t_zero_right.SetValues(new[] { 14051.0, 13883.0, 13580.0 });
        theta_t_zero_right.SetValues(new[] { initvalue1, initvalue2, initvalue3 });
        theta_initial_passive.SetValues(new[] { -54.73, -54.73, -54.73 }); 

        //Console.WriteLine("Successfully initialized.");
        //Console.WriteLine("Press any key to write");
        //Console.ReadKey();  

        WriteMessages();
        ReadMessages();
    }

    /// <summary>
    /// Read encoder values for one step 
    /// </summary>
    /// <returns></returns>
    public void ReadEncoderStep()
    {
        WriteMessages();

        ReadMessages();

        //for (int i = 0; i < 2; i++)
        //{
        //    ReadMessages(); 
        //}
        if (encoder_flag == 0){
            encoder_flag = 1;
            encoder_last.SetValues(new[] { theta_1_t, theta_2_t, theta_3_t });
            // Debug.Log("init encoder_last:" +  encoder_last[0] + ", " +  encoder_last[1] +", " +  encoder_last[2] );   
        }

        //right
        theta_t_list_ori_right.SetValues(new[] { theta_1_t, theta_2_t, theta_3_t });
        // theta_t_list_right = (DenseVector)(theta_t_list_ori_right - theta_t_zero_right).PointwiseMultiply(trans_ratio_list_right) + theta_initial_passive;

        // 优化编码器在零位附近值在0与最大值之间的变化问题
        encoder_increment_data =  get_encoder_increment(theta_t_list_ori_right, theta_t_zero_right);   // overflow_direction 判断当前运动时，编码器数值是增加(up)，还是减小（down）
        theta_t_list_right = (DenseVector)(encoder_increment_data).PointwiseMultiply(trans_ratio_list_right) + theta_initial_passive;

        x_list_t_right = cal_v_list(x_list_right, theta_t_list_right);
        ee_n_right = cal_pose(x_list_t_right);

        FE_angle_right = cal_fe_angle(x_list_t_right);
        x_list_right = (DenseVector)x_list_t_right.Clone();

        var R_matrix_right = VectorToRotateMatrix(ori_n, ee_n_right); 
        rotate_angle_right = MatrixToEuler(R_matrix_right) * radian_to_angle;

        //left
        theta_t_list_ori_left.SetValues(new[] { theta_4_t, theta_5_t, theta_6_t });
        theta_t_list_left = (DenseVector)(theta_t_list_ori_left - theta_t_zero_left).PointwiseMultiply(trans_ratio_list_left) + theta_initial_passive;

        // 优化编码器在零位附近值在0与最大值之间的变化问题
        // encoder_increment_data =  get_encoder_increment(theta_t_list_ori_left, theta_t_zero_left);   // overflow_direction 判断当前运动时，编码器数值是增加(up)，还是减小（down）
        // theta_t_list_left = (DenseVector)(encoder_increment_data).PointwiseMultiply(trans_ratio_list_left) + theta_initial_passive;


        x_list_t_left = cal_v_list(x_list_left, theta_t_list_left);
        ee_n_left = cal_pose(x_list_t_left);

        FE_angle_left = cal_fe_angle(x_list_t_left);
        x_list_left = (DenseVector)x_list_t_left.Clone();

        var R_matrix_left = VectorToRotateMatrix(ori_n, ee_n_left);
        rotate_angle_left = MatrixToEuler(R_matrix_left) * radian_to_angle;

        if (SaveFile)
        {
            current_time = DateTime.Now.Ticks;
            ee_pose_unity.WriteLine(((current_time - start_time) / 10000000.0).ToString() + "," + Convert.ToString(rotate_angle_right[0]) + "," + Convert.ToString(rotate_angle_right[1]) + "," + Convert.ToString(FE_angle_right) + "," + Convert.ToString(rotate_angle_left[0]) + "," + Convert.ToString(rotate_angle_left[1]) + "," + Convert.ToString(FE_angle_left));
            encoder_data_unity.WriteLine(((current_time - start_time) / 10000000.0).ToString() + "," + Convert.ToString(theta_t_list_right[0]) + "," + Convert.ToString(theta_t_list_right[1]) + "," + Convert.ToString(theta_t_list_right[2]) + "," + Convert.ToString(theta_1_t) + "," + Convert.ToString(theta_2_t) + "," + Convert.ToString(theta_3_t));
        }

    }

    /// <summary>
    /// Read Zero Angle
    /// </summary>
    /// <returns></returns>
    public DenseVector ReadZeroValue()
    {
        Console.Clear();
        WriteMessages();
        ReadMessages();
        
       //right
        theta_t_list_ori_right.SetValues(new[] { theta_1_t, theta_2_t, theta_3_t });
        theta_t_list_right = (DenseVector)(theta_t_list_ori_right - theta_t_zero_right).PointwiseMultiply(trans_ratio_list_right) + theta_initial_passive;

        x_list_t_right = cal_v_list(x_list_right, theta_t_list_right);
        ee_n_right = cal_pose(x_list_t_right);

        FE_angle_right = cal_fe_angle(x_list_t_right);
        x_list_right = (DenseVector)x_list_t_right.Clone();

        var R_matrix_right = VectorToRotateMatrix(ori_n, ee_n_right);
        rotate_angle_right = MatrixToEuler(R_matrix_right) * radian_to_angle;

        Debug.Log("Theta_T_Zero!!! :" + theta_t_zero_right.ToString());

        return theta_t_zero_right;
    }

    /// <summary>
    ///  process encoder angle
    /// </summary>
    /// <param name="msg"></param>
    /// <returns></returns>
    private double ReadEncoderAngle(TPCANMsg msg)
    {
        uint d_0 = Convert.ToByte(msg.DATA[3]);
        uint d_1 = Convert.ToByte(msg.DATA[4]);
        uint d_2 = Convert.ToByte(msg.DATA[5]);
        uint d_3 = Convert.ToByte(msg.DATA[6]);
        angle = (d_3 << 24) | (d_2 << 16) | (d_1 << 8) | d_0;

        /// interpret angles 
        if (msg.DATA[1].ToString() == "1" && msg.DATA[0].ToString() == "7")
        {
            theta_1_t = Convert.ToDouble(angle);
        }
        else if (msg.DATA[1].ToString() == "2" && msg.DATA[0].ToString() == "7")
        {
            theta_2_t = Convert.ToDouble(angle);
        }
        else if (msg.DATA[1].ToString() == "3" && msg.DATA[0].ToString() == "7")
        {
            theta_3_t = Convert.ToDouble(angle);
        }
        if (msg.DATA[1].ToString() == "4" && msg.DATA[0].ToString() == "7")
        {
            theta_4_t = Convert.ToDouble(angle);
        }
        else if (msg.DATA[1].ToString() == "5" && msg.DATA[0].ToString() == "7")
        {
            theta_5_t = Convert.ToDouble(angle);
        }
        else if (msg.DATA[1].ToString() == "6" && msg.DATA[0].ToString() == "7")
        {
            theta_6_t = Convert.ToDouble(angle);
        }
        else
        {
            Console.Write("Unknown CAN Message !!!");
        }

        return Convert.ToDouble(angle);
    }

    /// <summary>
    /// Reset CAN BUS
    /// </summary>
    public void reset()
    {
        PCANBasic.Reset(PcanHandle);
    }

    #region Main-Functions
    /// <summary>
    /// Function for writing PCAN-Basic messages
    /// </summary>
    private void WriteMessages()
    {
        TPCANStatus stsResult;

        if (IsFD)
            stsResult = WriteMessageFD();
        else
        {
            stsResult = WriteMessage(can_id_1);
            stsResult = WriteMessage(can_id_2);
            stsResult = WriteMessage(can_id_3);

            //stsResult = WriteMessage(can_id_4);
            //stsResult = WriteMessage(can_id_5);
            //stsResult = WriteMessage(can_id_6);
        }

        // Checks if the message was sent
        if (stsResult != TPCANStatus.PCAN_ERROR_OK)
            ShowStatus(stsResult);
        else
            Console.WriteLine("Message was successfully SENT");
    }

    /// <summary>
    /// Function for reading PCAN-Basic messages
    /// </summary>
    private void ReadMessages()
    {
        // Console.WriteLine("Message was successfully read!!!");
        TPCANStatus stsResult;

        // We read at least one time the queue looking for messages. If a message is found, we look again trying to 
        // find more. If the queue is empty or an error occurr, we get out from the dowhile statement.
        do
        {
            stsResult = IsFD ? ReadMessageFD() : ReadMessage();
            if (stsResult != TPCANStatus.PCAN_ERROR_OK && stsResult != TPCANStatus.PCAN_ERROR_QRCVEMPTY)
            {
                ShowStatus(stsResult);
                return;
            }
        } while ((!Convert.ToBoolean(stsResult & TPCANStatus.PCAN_ERROR_QRCVEMPTY)));
    }

    /// <summary>
    /// Function for reading messages on CAN-FD devices
    /// </summary>
    /// <returns>A TPCANStatus error code</returns>
    private TPCANStatus ReadMessageFD()
    {
        // We execute the "Read" function of the PCANBasic    
        TPCANStatus stsResult = PCANBasic.ReadFD(PcanHandle, out TPCANMsgFD CANMsg, out TPCANTimestampFD CANTimeStamp);
        if (stsResult != TPCANStatus.PCAN_ERROR_QRCVEMPTY)
            // We process the received message
            ProcessMessageCanFd(CANMsg, CANTimeStamp);

        return stsResult;
    }

    /// <summary>
    /// Function for reading CAN messages on normal CAN devices
    /// </summary>
    /// <returns>A TPCANStatus error code</returns> 
    private TPCANStatus ReadMessage()
    {
        Console.WriteLine("Message was successfully read second !!!");
        // We execute the "Read" function of the PCANBasic   
        TPCANStatus stsResult = PCANBasic.Read(PcanHandle, out TPCANMsg CANMsg, out TPCANTimestamp CANTimeStamp);
        if (stsResult != TPCANStatus.PCAN_ERROR_QRCVEMPTY)
            // We process the received message
            ProcessMessageCan(CANMsg, CANTimeStamp);

        return stsResult;
    }

    /// <summary>  
    /// Processes a received CAN message  
    /// </summary>   
    /// <param name="msg">The received PCAN-Basic CAN message</param>        
    /// <param name="itsTimeStamp">Timestamp of the message as TPCANTimestamp structure</param>   
    private void ProcessMessageCan(TPCANMsg msg, TPCANTimestamp itsTimeStamp)
    {
        Console.WriteLine("Message was successfully read third !!!");

        ulong microsTimestamp = Convert.ToUInt64(itsTimeStamp.micros + 1000 * itsTimeStamp.millis + 0x100000000 * 1000 * itsTimeStamp.millis_overflow);

        Console.WriteLine("Type: " + GetMsgTypeString(msg.MSGTYPE));
        Console.WriteLine("ID: " + GetIdString(msg.ID, msg.MSGTYPE));
        Console.WriteLine("Length: " + msg.LEN.ToString());
        Console.WriteLine("Time: " + GetTimeString(microsTimestamp));
        Console.WriteLine("Data: " + GetDataString(msg.DATA, msg.MSGTYPE, msg.LEN));
        Console.WriteLine("----------------------------------------------------------");

        // interpret receive angles  
        ReadEncoderAngle(msg);
    }

    /// <summary>
    /// Function for writing messages on CAN devices
    /// </summary>
    /// <returns>A TPCANStatus error code</returns>
    private TPCANStatus WriteMessage(byte can_id)
    {
        // Sends a CAN message with extended ID, and 8 data bytes
        var msgCanMessage = new TPCANMsg();

        int length = 4;
        msgCanMessage.DATA = new byte[length];
        msgCanMessage.ID = can_id;
        msgCanMessage.LEN = Convert.ToByte(length);

        // msgCanMessage.MSGTYPE = TPCANMessageType.PCAN_MESSAGE_EXTENDED;   
        msgCanMessage.MSGTYPE = TPCANMessageType.PCAN_MESSAGE_STANDARD;

        for (byte i = 0; i < length; i++)
        {
            msgCanMessage.DATA[i] = 0x00;
        }
        msgCanMessage.DATA[0] = 0x04;
        msgCanMessage.DATA[1] = can_id;
        msgCanMessage.DATA[2] = 0x01;

        return PCANBasic.Write(PcanHandle, ref msgCanMessage);
    }

    /// <summary> 
    /// Processes a received CAN-FD message 
    /// </summary>  
    /// <param name="msg">The received PCAN-Basic CAN-FD message</param>  
    /// <param name="itsTimeStamp">Timestamp of the message as microseconds (ulong)</param>  
    private void ProcessMessageCanFd(TPCANMsgFD msg, TPCANTimestampFD itsTimeStamp)
    {
        Console.WriteLine("Type: " + GetMsgTypeString(msg.MSGTYPE));
        Console.WriteLine("ID: " + GetIdString(msg.ID, msg.MSGTYPE));
        Console.WriteLine("Length: " + GetLengthFromDLC(msg.DLC).ToString());
        Console.WriteLine("Time: " + GetTimeString(itsTimeStamp));
        Console.WriteLine("Data: " + GetDataString(msg.DATA, msg.MSGTYPE, GetLengthFromDLC(msg.DLC)));
        Console.WriteLine("----------------------------------------------------------");
    }

    /// <summary>  
    /// Function for writing messages on CAN-FD devices
    /// </summary>  
    /// <returns>A TPCANStatus error code</returns>  
    private TPCANStatus WriteMessageFD()
    {
        // Sends a CAN-FD message with standard ID, 64 data bytes, and bitrate switch
        var msgCanMessageFD = new TPCANMsgFD();
        msgCanMessageFD.DATA = new byte[64];
        msgCanMessageFD.ID = 0x100;
        msgCanMessageFD.DLC = 15;
        msgCanMessageFD.MSGTYPE = TPCANMessageType.PCAN_MESSAGE_FD | TPCANMessageType.PCAN_MESSAGE_BRS;
        for (byte i = 0; i < 64; i++)
        {
            msgCanMessageFD.DATA[i] = i;
        }
        return PCANBasic.WriteFD(PcanHandle, ref msgCanMessageFD);
    }
    #endregion

    #region Help-Functions
    /// <summary>
    /// Gets the string representation of the type of a CAN message
    /// </summary>
    /// <param name="msgType">Type of a CAN message</param>
    /// <returns>The type of the CAN message as string</returns>
    private string GetMsgTypeString(TPCANMessageType msgType)
    {
        if ((msgType & TPCANMessageType.PCAN_MESSAGE_STATUS) == TPCANMessageType.PCAN_MESSAGE_STATUS)
            return "STATUS";

        if ((msgType & TPCANMessageType.PCAN_MESSAGE_ERRFRAME) == TPCANMessageType.PCAN_MESSAGE_ERRFRAME)
            return "ERROR";

        string strTemp;
        if ((msgType & TPCANMessageType.PCAN_MESSAGE_EXTENDED) == TPCANMessageType.PCAN_MESSAGE_EXTENDED)
            strTemp = "EXT";
        else
            strTemp = "STD";

        if ((msgType & TPCANMessageType.PCAN_MESSAGE_RTR) == TPCANMessageType.PCAN_MESSAGE_RTR)
            strTemp += "/RTR";
        else
            if ((int)msgType > (int)TPCANMessageType.PCAN_MESSAGE_EXTENDED)
        {
            strTemp += " [ ";
            if ((msgType & TPCANMessageType.PCAN_MESSAGE_FD) == TPCANMessageType.PCAN_MESSAGE_FD)
                strTemp += " FD";
            if ((msgType & TPCANMessageType.PCAN_MESSAGE_BRS) == TPCANMessageType.PCAN_MESSAGE_BRS)
                strTemp += " BRS";
            if ((msgType & TPCANMessageType.PCAN_MESSAGE_ESI) == TPCANMessageType.PCAN_MESSAGE_ESI)
                strTemp += " ESI";
            strTemp += " ]";
        }

        return strTemp;
    }

    /// <summary> 
    /// Gets the string representation of the ID of a CAN message
    /// </summary>
    /// <param name="id">Id to be parsed</param>
    /// <param name="msgType">Type flags of the message the Id belong</param>
    /// <returns>Hexadecimal representation of the ID of a CAN message</returns>
    private string GetIdString(uint id, TPCANMessageType msgType)
    {
        if ((msgType & TPCANMessageType.PCAN_MESSAGE_EXTENDED) == TPCANMessageType.PCAN_MESSAGE_EXTENDED)
            return string.Format("{0:X8}h", id);

        return string.Format("{0:X3}h", id);
    }

    /// <summary>
    /// Gets the data length of a CAN message
    /// </summary>
    /// <param name="dlc">Data length code of a CAN message</param>
    /// <returns>Data length as integer represented by the given DLC code</returns>
    private int GetLengthFromDLC(byte dlc)
    {
        switch (dlc)
        {
            case 9: return 12;
            case 10: return 16;
            case 11: return 20;
            case 12: return 24;
            case 13: return 32;
            case 14: return 48;
            case 15: return 64;
            default: return dlc;
        }
    }

    /// <summary>
    /// Gets the string representation of the timestamp of a CAN message, in milliseconds
    /// </summary>
    /// <param name="time">Timestamp in microseconds</param>
    /// <returns>String representing the timestamp in milliseconds</returns>
    private string GetTimeString(TPCANTimestampFD time)
    {
        double fTime = (time / 1000.0);
        return fTime.ToString("F1");
    }

    /// <summary>
    /// Gets the data of a CAN message as a string
    /// </summary> 
    /// <param name="data">Array of bytes containing the data to parse</param>
    /// <param name="msgType">Type flags of the message the data belong</param>
    /// <param name="dataLength">The amount of bytes to take into account wihtin the given data</param>
    /// <returns>A string with hexadecimal formatted data bytes of a CAN message</returns>
    private string GetDataString(byte[] data, TPCANMessageType msgType, int dataLength)
    {
        if ((msgType & TPCANMessageType.PCAN_MESSAGE_RTR) == TPCANMessageType.PCAN_MESSAGE_RTR)
            return "Remote Request";
        else
        {
            string strTemp = "";
            for (int i = 0; i < dataLength; i++)
                strTemp += string.Format("{0:X2} ", data[i]);
            return strTemp;
        }
    }

    /// <summary>
    /// Checks for availability of the PCANBasic labrary
    /// </summary>
    /// <returns>If the library was found or not</returns>
    private bool CheckForLibrary()
    {
        // Check for dll file
        try
        {
            PCANBasic.Uninitialize(PCANBasic.PCAN_NONEBUS);
            return true;
        }
        catch (DllNotFoundException)
        {
            Console.WriteLine("Unable to find the library: PCANBasic.dll !");
            Console.WriteLine("Press any key to close");
            Console.ReadKey();
        }

        return false;
    }

    /// <summary>
    /// Shows/prints the configurable parameters for this sample and information about them
    /// </summary>
    private void ShowConfigurationHelp()
    {
        Console.WriteLine("=========================================================================================");
        Console.WriteLine("|                          PCAN-Basic ManualWrite Example                                |");
        Console.WriteLine("=========================================================================================");
        Console.WriteLine("Following parameters are to be adjusted before launching, according to the hardware used |");
        Console.WriteLine("                                                                                         |");
        Console.WriteLine("* PcanHandle: Numeric value that represents the handle of the PCAN-Basic channel to use. |");
        Console.WriteLine("              See 'PCAN-Handle Definitions' within the documentation                     |");
        Console.WriteLine("* IsFD: Boolean value that indicates the communication mode, CAN (false) or CAN-FD (true)|");
        Console.WriteLine("* Bitrate: Numeric value that represents the BTR0/BR1 bitrate value to be used for CAN   |");
        Console.WriteLine("           communication                                                                 |");
        Console.WriteLine("* BitrateFD: String value that represents the nominal/data bitrate value to be used for  |");
        Console.WriteLine("             CAN-FD communication                                                        |");
        Console.WriteLine("=========================================================================================");
        Console.WriteLine("");
    }

    /// <summary>
    /// Shows/prints the configured paramters
    /// </summary>
    private void ShowCurrentConfiguration()
    {
        Console.WriteLine("Parameter values used");
        Console.WriteLine("----------------------");
        Console.WriteLine("* PCANHandle: " + FormatChannelName(PcanHandle, IsFD));
        Console.WriteLine("* IsFD: " + IsFD);
        Console.WriteLine("* Bitrate: " + ConvertBitrateToString(Bitrate));
        Console.WriteLine("* BitrateFD: " + BitrateFD);
        Console.WriteLine("");
    }

    /// <summary>
    /// Shows formatted status
    /// </summary>
    /// <param name="status">Will be formatted</param>
    private void ShowStatus(TPCANStatus status)
    {
        Console.WriteLine("=========================================================================================");
        Console.WriteLine(GetFormattedError(status));
        Console.WriteLine("=========================================================================================");
    }

    /// <summary>
    /// Gets the formatted text for a PCAN-Basic channel handle
    /// </summary>
    /// <param name="handle">PCAN-Basic Handle to format</param>
    /// <param name="isFD">If the channel is FD capable</param>
    /// <returns>The formatted text for a channel</returns>
    private string FormatChannelName(TPCANHandle handle, bool isFD)
    {
        TPCANDevice devDevice;
        byte byChannel;

        // Gets the owner device and channel for a PCAN-Basic handle
        if (handle < 0x100)
        {
            devDevice = (TPCANDevice)(handle >> 4);
            byChannel = (byte)(handle & 0xF);
        }
        else
        {
            devDevice = (TPCANDevice)(handle >> 8);
            byChannel = (byte)(handle & 0xFF);
        }

        // Constructs the PCAN-Basic Channel name and return it
        if (isFD)
            return string.Format("{0}:FD {1} ({2:X2}h)", devDevice, byChannel, handle);
        return string.Format("{0} {1} ({2:X2}h)", devDevice, byChannel, handle);
    }

    /// <summary>
    /// Help Function used to get an error as text
    /// </summary>
    /// <param name="error">Error code to be translated</param>
    /// <returns>A text with the translated error</returns>
    private string GetFormattedError(TPCANStatus error)
    {
        // Creates a buffer big enough for a error-text
        var strTemp = new StringBuilder(256);
        // Gets the text using the GetErrorText API function. If the function success, the translated error is returned. 
        // If it fails, a text describing the current error is returned.
        if (PCANBasic.GetErrorText(error, 0x09, strTemp) != TPCANStatus.PCAN_ERROR_OK)
            return string.Format("An error occurred. Error-code's text ({0:X}) couldn't be retrieved", error);
        return strTemp.ToString();
    }

    /// <summary>
    /// Convert bitrate c_short value to readable string
    /// </summary>  
    /// <param name="bitrate">Bitrate to be converted</param>  
    /// <returns>A text with the converted bitrate</returns>  
    private string ConvertBitrateToString(TPCANBaudrate bitrate)
    {
        switch (bitrate)
        {
            case TPCANBaudrate.PCAN_BAUD_1M:
                return "1 MBit/sec";
            case TPCANBaudrate.PCAN_BAUD_800K:
                return "800 kBit/sec";
            case TPCANBaudrate.PCAN_BAUD_500K:
                return "500 kBit/sec";
            case TPCANBaudrate.PCAN_BAUD_250K:
                return "250 kBit/sec";
            case TPCANBaudrate.PCAN_BAUD_125K:
                return "125 kBit/sec";
            case TPCANBaudrate.PCAN_BAUD_100K:
                return "100 kBit/sec";
            case TPCANBaudrate.PCAN_BAUD_95K:
                return "95,238 kBit/sec";
            case TPCANBaudrate.PCAN_BAUD_83K:
                return "83,333 kBit/sec";
            case TPCANBaudrate.PCAN_BAUD_50K:
                return "50 kBit/sec";
            case TPCANBaudrate.PCAN_BAUD_47K:
                return "47,619 kBit/sec";
            case TPCANBaudrate.PCAN_BAUD_33K:
                return "33,333 kBit/sec";
            case TPCANBaudrate.PCAN_BAUD_20K:
                return "20 kBit/sec";
            case TPCANBaudrate.PCAN_BAUD_10K:
                return "10 kBit/sec";
            case TPCANBaudrate.PCAN_BAUD_5K:
                return "5 kBit/sec";
            default:
                return "Unknown Bitrate";
        }
    }

    //////////////////////////////////////////////////////////////////
    /// <math> 
    /// Forward Kinematics Calculation
    /// </math> 
    //////////////////////////////////////////////////////////////////
    private DenseVector cal_v_list(DenseVector x_list, DenseVector theta_t_list)
    {
        // iteration calculation Newton 
        int Num_iteration = 100;
        var x_list_t = x_list.Clone();

        // calculate w list  
        var w_matrix = cal_val_w(theta_t_list);

        for (int i = 0; i < Num_iteration; i = i + 1)
        {
            var FF_value = cal_FF(x_list, w_matrix);

            var J_matrix = cal_Jacobian(x_list, w_matrix);

            x_list_t = x_list - (J_matrix.Inverse()) * FF_value;

            if (cal_norm((DenseVector)x_list_t - x_list) < 0.01)
            {
                break;
            }

            x_list = (DenseVector)x_list_t.Clone();
        }
        return (DenseVector)x_list_t;
    }

    /// <summary>
    /// cal pose from v_list 
    /// </summary>
    /// <param name="x_list"></param>
    private DenseVector cal_pose(DenseVector x_list)
    {
        var v_t_1 = x_list.SubVector(0, 3) * SPM_R_2;
        var v_t_2 = x_list.SubVector(3, 3) * SPM_R_2;
        var v_t_3 = x_list.SubVector(6, 3) * SPM_R_2;

        // Console.WriteLine("Three v_t_1 :" + v_t_1.ToString() + ", v_t_2 :" + v_t_2.ToString() + ", v_t_3 :" + v_t_3.ToString()); 
        //double RU_angle = 0.0;    
        //double PS_angle = 0.0;    
        //double FE_angle_right = 0.0;    

        var v_t_1_initial = new DenseVector(3);

        var n_sum = v_t_1 + v_t_2 + v_t_3;
        // var n = n_sum/Math.Sqrt(Math.Pow(n_sum[0], 2) + Math.Pow(n_sum[1], 2) + Math.Pow(n_sum[2], 2));   
        var n = cal_normalzied_vec((DenseVector)n_sum) * SPM_R;

        v_t_1_initial = cal_initial_vec(n, 54.7356);

        // radian   
        if (n[0] < 0)
        {
            RU_angle = Math.Acos(n[2] / cal_norm((DenseVector)n));
        }
        else
        {
            RU_angle = 2 * Math.PI - Math.Acos(n[2] / cal_norm((DenseVector)n));
        }

        RU_angle = RU_angle * radian_to_angle;
        // Console.WriteLine("Normal vec:" + n.ToString());  

        // PS_angle = Math.Atan(n[1] / n[0]); 
        PS_angle = Math.Atan2(Math.Round(n[1], 5), Math.Round(n[0], 5)) * radian_to_angle;

        //// calculate FE_angle_right
        //var rotate_vec_fe = cal_cross_vector((DenseVector)v_t_1_initial, (DenseVector)v_t_1);
        //if (rotate_vec_fe.DotProduct(n) > 0)
        //{
        //    FE_angle_right = cal_vecs_angle(v_t_1_initial - n, (DenseVector)v_t_1 - n) * radian_to_angle;
        //}
        //else
        //{
        //    FE_angle_right = -1 * cal_vecs_angle(v_t_1_initial - n, (DenseVector)v_t_1 - n) * radian_to_angle;
        //}

        // FE_angle_right = cal_vecs_angle(v_t_1_initial - n, (DenseVector)v_t_1 - n) * radian_to_angle;
        // Console.WriteLine("Vec 1:" + v_t_1_initial.ToString() + "Vec 2:" + v_t_1.ToString());  

        // FE_angle_right = Math.Acos((v_t_1_initial - n).DotProduct(v_t_1 - n) / cal_norm((DenseVector)(v_t_1 - n)) / cal_norm((DenseVector)(v_t_1_initial - n)));
        // Console.WriteLine("Three Angles RU :" + RU_angle.ToString() + ", PS :" + PS_angle.ToString() + ", FE :" + FE_angle_right.ToString());  

        return n;
    }

    /// <summary>
    /// cal pose from v_list 
    /// </summary>
    /// <param name="x_list"></param>
    private double cal_fe_angle(DenseVector x_list)
    {
        // var n = new DenseVector(3);  
        var v_t_1 = x_list.SubVector(0, 3) * SPM_R_2;
        var v_t_2 = x_list.SubVector(3, 3) * SPM_R_2;
        var v_t_3 = x_list.SubVector(6, 3) * SPM_R_2;

        // Console.WriteLine("Three v_t_1 :" + v_t_1.ToString() + ", v_t_2 :" + v_t_2.ToString() + ", v_t_3 :" + v_t_3.ToString()); 
        //double RU_angle = 0.0;    
        //double PS_angle = 0.0;    
        //double FE_angle_right = 0.0;    

        var v_t_1_initial = new DenseVector(3);

        var n_sum = v_t_1 + v_t_2 + v_t_3;
        // var n = n_sum/Math.Sqrt(Math.Pow(n_sum[0], 2) + Math.Pow(n_sum[1], 2) + Math.Pow(n_sum[2], 2));   
        var n = cal_normalzied_vec((DenseVector)n_sum) * SPM_R;

        //v_t_1_initial = cal_initial_vec(n, 54.7356);
        v_t_1_initial = cal_initial_vec(n, 0);

        //// radian   
        //if (n[0] < 0)
        //{
        //    RU_angle = Math.Acos(n[2] / cal_norm((DenseVector)n));
        //}
        //else
        //{
        //    RU_angle = 2 * Math.PI - Math.Acos(n[2] / cal_norm((DenseVector)n));
        //}

        //RU_angle = RU_angle * radian_to_angle;  
        //// Console.WriteLine("Normal vec:" + n.ToString());  

        //// PS_angle = Math.Atan(n[1] / n[0]); 
        //PS_angle = Math.Atan2(Math.Round(n[1], 5), Math.Round(n[0], 5)) * radian_to_angle; 

        // calculate FE_angle_right;  
        // FE_angle_right = cal_vecs_angle(v_t_1_initial - n, (DenseVector)v_t_1 - n) * radian_to_angle;
        // Console.WriteLine("Vec 1:" + v_t_1_initial.ToString() + "Vec 2:" + v_t_1.ToString());  
        var FE_angle = 0.0;

        var rotate_vec_fe = cal_cross_vector((DenseVector)v_t_1_initial, (DenseVector)v_t_1);
        if (rotate_vec_fe.DotProduct(n) > 0)
        {
            FE_angle = cal_vecs_angle(v_t_1_initial - n, (DenseVector)v_t_1 - n) * radian_to_angle;
        }
        else
        {
            FE_angle = -1 * cal_vecs_angle(v_t_1_initial - n, (DenseVector)v_t_1 - n) * radian_to_angle;
        }

        // FE_angle_right = Math.Acos((v_t_1_initial - n).DotProduct(v_t_1 - n) / cal_norm((DenseVector)(v_t_1 - n)) / cal_norm((DenseVector)(v_t_1_initial - n)));
        // Console.WriteLine("Three Angles RU :" + RU_angle.ToString() + ", PS :" + PS_angle.ToString() + ", FE :" + FE_angle_right.ToString());

        return FE_angle;
    }

    /// <summary>
    /// Calculate Rotate Matrix from Rotate Vectors
    /// </summary>
    /// <param name="v_before"></param>
    /// <param name="v_after"></param>
    /// <returns></returns>
    private DenseMatrix VectorToRotateMatrix(DenseVector v_before, DenseVector v_after)
    {
        var rotate_vec = new DenseVector(3);
        var rotate_matrix = new DenseMatrix(3, 3);

        double dot_angle = cal_vecs_angle(v_before, v_after);
        rotate_vec = cal_cross_vector(v_before, v_after);
        rotate_vec = cal_normalzied_vec(rotate_vec);

        rotate_matrix[0, 0] = Math.Cos(dot_angle) + rotate_vec[0] * rotate_vec[1] * (1 - Math.Cos(dot_angle));
        rotate_matrix[0, 1] = rotate_vec[0] * rotate_vec[1] * (1 - Math.Cos(dot_angle) - rotate_vec[2] * Math.Sin(dot_angle));
        rotate_matrix[0, 2] = rotate_vec[1] * Math.Sin(dot_angle) + rotate_vec[0] * rotate_vec[2] * (1 - Math.Cos(dot_angle));

        rotate_matrix[1, 0] = rotate_vec[2] * Math.Sin(dot_angle) + rotate_vec[0] * rotate_vec[1] * (1 - Math.Cos(dot_angle));
        rotate_matrix[1, 1] = Math.Cos(dot_angle) + rotate_vec[1] * rotate_vec[1] * (1 - Math.Cos(dot_angle));
        rotate_matrix[1, 2] = -1 * rotate_vec[0] * Math.Sin(dot_angle) + rotate_vec[1] * rotate_vec[2] * (1 - Math.Cos(dot_angle));

        rotate_matrix[2, 0] = -1 * rotate_vec[1] * Math.Sin(dot_angle) + rotate_vec[0] * rotate_vec[2] * (1 - Math.Cos(dot_angle));
        rotate_matrix[2, 1] = rotate_vec[0] * Math.Sin(dot_angle) + rotate_vec[1] * rotate_vec[2] * (1 - Math.Cos(dot_angle));
        rotate_matrix[2, 2] = Math.Cos(dot_angle) + rotate_vec[2] * rotate_vec[2] * (1 - Math.Cos(dot_angle));

        return rotate_matrix;
    }

    /// <summary>
    /// Calculate Rotate Matrix from angles
    /// </summary>
    /// <param name="phi"></param>
    /// <param name="theta"></param>
    /// <param name="sigma"></param>
    /// <returns></returns>
    private static DenseMatrix RotateMatrix(double phi, double theta, double sigma)
    {
        // z: phi, y: theta, z : sigma - phi
        DenseMatrix R = new DenseMatrix(3, 3);

        R[0, 0] = Math.Cos(phi) * Math.Cos(theta) * Math.Cos(sigma - phi) - Math.Sin(phi) * Math.Sin(sigma - phi);
        R[0, 1] = -1 * Math.Cos(phi) * Math.Cos(theta) * Math.Sin(sigma - phi) - Math.Sin(phi) * Math.Cos(sigma - phi);
        R[0, 2] = Math.Cos(phi) * Math.Sin(theta);

        R[1, 0] = Math.Sin(phi) * Math.Cos(theta) * Math.Cos(sigma - phi) + Math.Cos(phi) * Math.Sin(sigma - phi);
        R[1, 1] = -1 * Math.Sin(phi) * Math.Cos(theta) * Math.Sin(sigma - phi) + Math.Cos(phi) * Math.Cos(sigma - phi);
        R[1, 2] = Math.Sin(phi) * Math.Sin(theta);

        R[2, 0] = -1 * Math.Sin(theta) * Math.Cos(sigma - phi);
        R[2, 1] = Math.Sin(theta) * Math.Sin(sigma - phi);
        R[2, 2] = Math.Cos(theta);

        return R;
    }

    /// <summary> 
    /// Calculate Euler Angles from Matrix 
    /// </summary>
    /// <param name="R"></param>
    /// <returns></returns>
    private static DenseVector MatrixToEuler(DenseMatrix R)
    {
        DenseVector rotate_theta_list = new DenseVector(3);

        // calculate rotate theta
        rotate_theta_list[0] = Math.Atan2(R[2, 1], R[2, 2]);   // roll
        rotate_theta_list[1] = Math.Atan2(-1 * R[2, 0], Math.Sqrt(Math.Pow(R[2, 1], 2) + Math.Pow(R[2, 2], 2)));  // pitch
        rotate_theta_list[2] = Math.Atan2(R[1, 0], R[0, 0]);  // yaw

        return rotate_theta_list;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="n"></param>
    /// <param name="initial_theta"></param>
    /// <returns></returns>
    private DenseVector cal_initial_vec(DenseVector n, double initial_theta)
    {
        var v_t_1_initial = new DenseVector(3);
        initial_theta = initial_theta * angle_to_radian;

        var a = new DenseVector(new[] { 0.0, 1.0, 0.0 });
        var b = new DenseVector(new[] { 1.0, 0.0, 0.0 });

        // var c = a.OuterProduct(b);  
        var c = cal_cross_vector(n, a);
        if (cal_norm(c) == 0)
        {
            c = cal_cross_vector(n, b);
        }
        var d = cal_cross_vector(n, c);

        c = cal_normalzied_vec(c);
        d = cal_normalzied_vec(d);

        v_t_1_initial[0] = n[0] + SPM_R_1 * c[0] * Math.Cos(initial_theta) + SPM_R_1 * d[0] * Math.Sin(initial_theta);
        v_t_1_initial[1] = n[1] + SPM_R_1 * c[1] * Math.Cos(initial_theta) + SPM_R_1 * d[1] * Math.Sin(initial_theta);
        v_t_1_initial[2] = n[2] + SPM_R_1 * c[2] * Math.Cos(initial_theta) + SPM_R_1 * d[2] * Math.Sin(initial_theta);

        return v_t_1_initial;
    }

    /// <summary>
    /// Calculate W vector
    /// </summary>
    /// <param name="theta_t_list"></param>
    /// <returns></returns>
    private DenseMatrix cal_val_w(DenseVector theta_t_list)
    {
        var vector_theta_reset = Vector<double>.Build.Dense(new[] { 0.0, 120.0, 240.0 });

        var vector_theta_t = Vector<double>.Build.Dense(new[] { 0.0, 0.0, 0.0 });
        vector_theta_t = (vector_theta_reset - theta_t_list) * angle_to_radian;

        var w_matrix = new DenseMatrix(3, 3);

        for (int i = 0; i < 3; i = i + 1)
        {
            var vector_w = Vector<double>.Build.Dense(new[] { Math.Sin(alpha_1) * Math.Cos(vector_theta_t[i]), Math.Sin(alpha_1) * Math.Sin(vector_theta_t[i]), -1 * Math.Cos(alpha_1) });
            // Console.WriteLine("a ��ֵ�� {0}", i);
            w_matrix.SetRow(i, vector_w);
        }

        return w_matrix;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="x_list"></param>
    /// <param name="w_matrix"></param>
    /// <returns></returns>
    private DenseVector cal_FF(DenseVector x_list, DenseMatrix w_matrix)
    {
        var F_vector = new DenseVector(9);

        var v_t_1 = x_list.SubVector(0, 3);
        var v_t_2 = x_list.SubVector(3, 3);
        var v_t_3 = x_list.SubVector(6, 3);

        var w_t_1 = w_matrix.Row(0);
        var w_t_2 = w_matrix.Row(1);
        var w_t_3 = w_matrix.Row(2);

        F_vector[0] = Math.Pow(v_t_1[0], 2) + Math.Pow(v_t_1[1], 2) + Math.Pow(v_t_1[2], 2) - 1;
        F_vector[1] = Math.Pow(v_t_2[0], 2) + Math.Pow(v_t_2[1], 2) + Math.Pow(v_t_2[2], 2) - 1;
        F_vector[2] = Math.Pow(v_t_3[0], 2) + Math.Pow(v_t_3[1], 2) + Math.Pow(v_t_3[2], 2) - 1;

        F_vector[3] = w_t_1.DotProduct(v_t_1) - Math.Cos(alpha_2);
        F_vector[4] = w_t_2.DotProduct(v_t_2) - Math.Cos(alpha_2);
        F_vector[5] = w_t_3.DotProduct(v_t_3) - Math.Cos(alpha_2);

        F_vector[6] = v_t_1.DotProduct(v_t_2) - Math.Cos(alpha_3);
        F_vector[7] = v_t_2.DotProduct(v_t_3) - Math.Cos(alpha_3);
        F_vector[8] = v_t_3.DotProduct(v_t_1) - Math.Cos(alpha_3);

        return F_vector;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="x_list"></param>
    /// <param name="w_matrix"></param>
    /// <returns></returns>
    private DenseMatrix cal_Jacobian(DenseVector x_list, DenseMatrix w_matrix)
    {
        var J_matrix = new DenseMatrix(9, 9);

        var v_t_1 = x_list.SubVector(0, 3);
        var v_t_2 = x_list.SubVector(3, 3);
        var v_t_3 = x_list.SubVector(6, 3);

        var w_t_1 = w_matrix.Row(0);
        var w_t_2 = w_matrix.Row(1);
        var w_t_3 = w_matrix.Row(2);

        // v_t_list
        J_matrix.SetRow(0, new[] { 2 * v_t_1[0], 2 * v_t_1[1], 2 * v_t_1[2], 0.0, 0.0, 0.0, 0.0, 0.0, 0.0 });
        J_matrix.SetRow(1, new[] { 0.0, 0.0, 0.0, 2 * v_t_2[0], 2 * v_t_2[1], 2 * v_t_2[2], 0.0, 0.0, 0.0 });
        J_matrix.SetRow(2, new[] { 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 2 * v_t_3[0], 2 * v_t_3[1], 2 * v_t_3[2] });

        // w_t_list
        J_matrix.SetRow(3, new[] { w_t_1[0], w_t_1[1], w_t_1[2], 0.0, 0.0, 0.0, 0.0, 0.0, 0.0 });
        J_matrix.SetRow(4, new[] { 0.0, 0.0, 0.0, w_t_2[0], w_t_2[1], w_t_2[2], 0.0, 0.0, 0.0 });
        J_matrix.SetRow(5, new[] { 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, w_t_3[0], w_t_3[1], w_t_3[2] });

        // two v_t_list 
        J_matrix.SetRow(6, new[] { v_t_2[0], v_t_2[1], v_t_2[2], v_t_1[0], v_t_1[1], v_t_1[2], 0.0, 0.0, 0.0 });
        J_matrix.SetRow(7, new[] { 0.0, 0.0, 0.0, v_t_3[0], v_t_3[1], v_t_3[2], v_t_2[0], v_t_2[1], v_t_2[2] });
        J_matrix.SetRow(8, new[] { v_t_3[0], v_t_3[1], v_t_3[2], 0.0, 0.0, 0.0, v_t_1[0], v_t_1[1], v_t_1[2] });

        return J_matrix;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="n"></param>
    /// <returns></returns>
    private double cal_norm(DenseVector n)
    {
        double norm_value = Math.Sqrt(Math.Pow(n[0], 2) + Math.Pow(n[1], 2) + Math.Pow(n[2], 2));

        return norm_value;
    }

    /// <summary>
    /// Output radian value
    /// </summary>
    /// <param name="n"></param>
    /// <param name="m"></param>
    /// <returns></returns>
    private double cal_vecs_angle(DenseVector n, DenseVector m)
    {
        double vecs_angle = Math.Acos(n.DotProduct(m) / cal_norm((DenseVector)(n)) / cal_norm((DenseVector)(m)));

        return vecs_angle;
    }

    /// <summary>
    /// cross vector of two given vectors
    /// </summary>
    /// <param name="n"></param>
    /// <param name="m"></param>
    /// <returns></returns>
    private DenseVector cal_cross_vector(DenseVector n, DenseVector m)
    {
        var cross_vector = new DenseVector(3);
        cross_vector[0] = n[1] * m[2] - n[2] * m[1];
        cross_vector[1] = n[2] * m[0] - n[0] * m[2];
        cross_vector[2] = n[0] * m[1] - n[1] * m[0];

        return cross_vector;
    }

    /// <summary>
    /// calculation normalization vector 
    /// </summary>
    /// <param name="n"></param>
    /// <returns></returns>
    private DenseVector cal_normalzied_vec(DenseVector n)
    {
        DenseVector normalized_vec = n / Math.Sqrt(Math.Pow(n[0], 2) + Math.Pow(n[1], 2) + Math.Pow(n[2], 2));
        return normalized_vec;
    }


        /*-------------------------------------------------------------
        整数变量x及其初始值x0，x的数据在区间[A,B]中，当x增加至B并继续增加时时，令值等于A再增加，
        当x小于A并持续减小时，则令其等于B再减小。如何得到x原本的结果, 然后计算相对x0的增加量    */

    private double get_single_encoder_increment(double x, double x0 , int cirl_num)
    {
        double adjusted_distance = 0, x_original = 0;

        switch (cirl_num)
        {
            case  1: //overflowDirection.up :     // 圈数+1
                // % 计算 x0 到 x 的相对距离
                adjusted_distance = matlab_mod(x - x0, (encoder_max - encoder_min + 1));   // % 调整相对距离至 [- (B - A), B - A] 区间内
                break;
            case -1: //overflowDirection.down :   // 圈数-1
                // distance = x0 - x;      // % 计算 x0 到 x 的相对距离
                adjusted_distance = matlab_mod(x0 - x, (encoder_max - encoder_min + 1));
                break;
            case 0: //overflowDirection.stay :    // 圈数为0，表示在有效范围内
                
                break;
        }
        
        // % 将调整后的相对距离加回到 x0 中，得到原始的 x 值
        switch (cirl_num)
        {
            case 1: //overflowDirection.up :
                x_original = x0 + adjusted_distance;
                break;
            case -1: //overflowDirection.down :
                x_original = x0 - adjusted_distance;
                break;
            case 0:
                x_original = x;
            break;

        }
        double increment = x_original - x0;

        return increment;

    }

    private DenseVector get_encoder_increment(DenseVector current, DenseVector init)
    {
        int i = 0;
        DenseVector data = new DenseVector(3);
        int A = encoder_min;
        int B = encoder_max;
        // Debug.Log("0000 encoder_last:" +  encoder_last[0] + ", " +  encoder_last[1] +", " +  encoder_last[2] +", current: " + current[0] + ", " +  current[1] +", " +  current[2]);   


        while(i < 3)
        {
            if ((B - encoder_last[i]) < 3000 && (current[i] - A) < 3000) {
                encoder_cirls[i] = encoder_cirls[i] + 1  ;        
            }
            else if ((encoder_last[i] - A) < 3000 && (B - current[i]) < 3000) {
                encoder_cirls[i] = encoder_cirls[i] - 1;
            } else {
                encoder_cirls[i] = encoder_cirls[i];
            }

            data[i] = get_single_encoder_increment(current[i], init[i], encoder_cirls[i]);
            encoder_last[i] = current[i];
            i++;
        }
        return data;

    }


    private double matlab_mod(double a, double m){
        if(m == 0){
            return a;
            
        }
         return  a - m*(int)Math.Floor(a/m);
    }

    #endregion

}
