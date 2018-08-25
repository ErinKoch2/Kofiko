/*
  trying to integrate UDP into this
  so that Kofiko can control the stimulus presentation
  UDP stuff based off of:
  https://stackoverflow.com/questions/37131742/how-to-use-udp-with-unity-methods
*/

using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections;
using System.Threading;
using System.IO.Ports;

public class SpaceDownUDP : MonoBehaviour {
  // for arduino
  SerialPort arduinoPort = new SerialPort("COM4",9600);


  // for fove
  [SerializeField]
  public FoveInterfaceBase foveInterface;
  Vector3 left_eye_vector;
  Vector3 right_eye_vector;

  int kofiko_port_number = 1111;
  int port_number = 12345;
  static UdpClient udp;
  static UdpClient udp_kofiko;
  Thread thread;
  static readonly object lockObject = new object();
  string returnData = "";

  bool triggered = false;

  bool show_calibration = true;
  bool show_surface = false;
  bool show_cross = false;

  float disappear_scale = 0.0001F;
  float rotation_amount = 25.0F; // in degrees
  // float x_distance = 7.0F;
  float z_distance = 7.0F;
  float displacement_magnitude = 1.0F; // the width of the square is 2.5 to begin with


  Vector3 init_surface_size;
  Vector3 init_cross_size;

  float rotation_y = 0.5f;
  // float rotation_z = 0.5f;
  float rotation_x = 0.5f;
  float translation_x = 0.5f;
  float translation_y = 0.5f;
  // float translation_z = 0.5f;

  Vector3 init_calibration_size;
  float calibration_scale;
  float calibration_disparity;

  IPEndPoint endPoint;


  private void ThreadMethod() {
    print ("This loop is running!");

    udp = new UdpClient(12345);
    udp_kofiko = new UdpClient("127.0.0.1", kofiko_port_number);

    while (true) {
      IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, port_number);

      byte[] receiveBytes = udp.Receive(ref RemoteIpEndPoint);

      lock (lockObject)
      {
        returnData = Encoding.ASCII.GetString(receiveBytes);
        string[] words = returnData.Split(',');

        triggered = true; // trigger is on for main loop update!
        if (words[0].Equals("surfaceOn")) {  // show new surface
          rotation_x = float.Parse(words[1]);
          rotation_y = float.Parse(words[2]);
          translation_x = float.Parse(words[3]);
          translation_y = float.Parse(words[4]);
          show_surface = true;
          show_cross = true;
        } else if (words[0].Equals("surfaceOff")) {  // turn off surface
          show_surface = false;
          show_calibration = false;
          show_cross = true;
        } else if (words[0].Equals("calibrationOn")) {
          translation_x = float.Parse(words[1]);
          translation_y = float.Parse(words[2]);
          calibration_scale = float.Parse(words[3]);
          calibration_disparity = float.Parse(words[4]);
          show_calibration = true;
          show_surface = false;
          show_cross = false;
        } else if (words[0].Equals("calibrationOff")) {
          show_surface = false;
          show_calibration = false;
          show_cross = false;
        }

        /* else if (words[0].Equals("getAnalog")) {
          // Debug.Log(left_eye_vector);
          // Debug.Log(right_eye_vector);
          // Debug.Log(left_eye_vector.x);
          // Debug.Log(left_eye_vector.y);
          // Debug.Log(left_eye_vector.z);

          double theta_L = -Math.Atan2( (double) left_eye_vector.x, (double) left_eye_vector.z);
          double rho_L = Math.Atan2((double) left_eye_vector.y, (double) Math.Sqrt(left_eye_vector.x*left_eye_vector.x + left_eye_vector.z*left_eye_vector.z));
          double theta_R = -Math.Atan2( (double) right_eye_vector.x, (double) right_eye_vector.z);
          double rho_R = Math.Atan2((double) right_eye_vector.y, (double) Math.Sqrt(right_eye_vector.x*right_eye_vector.x + right_eye_vector.z*right_eye_vector.z));

          // Debug.Log(theta_L);
          // Debug.Log(rho_L);
          // converts stuff into bytes
          string dataString = theta_L.ToString() + "," + rho_L.ToString() + "," + theta_R.ToString() + "," + rho_R.ToString();
          // Debug.Log(dataString);
          byte[] send_buffer = Encoding.ASCII.GetBytes(dataString);
          //byte[] unicodeBytes = Encoding.Unicode.GetBytes(unicodeString);
          //byte[] asciiBytes = Encoding.Convert(Encoding.Unicode, Encoding.ASCII, unicodeBytes);
          udp_kofiko.Send(send_buffer, send_buffer.Length);
          // Debug.Log("done sending!");


        } */

      }
    }
  }

  private void Start() {
    arduinoPort.Open();


    thread = new Thread(new ThreadStart(ThreadMethod));
    print ("starting loop");
    thread.Start();

    init_calibration_size = GameObject.Find ("calibration point").transform.localScale;
    init_surface_size = GameObject.Find ("surface").transform.localScale;
    init_cross_size = GameObject.Find ("fixation cross").transform.localScale;
    // init_fixation_point_size = // GameObject.Find ("fixation cross").transform.localScale;

    if (!(show_surface)) {
      GameObject.Find ("surface").transform.localScale = init_surface_size*disappear_scale;
    }

    endPoint = new IPEndPoint(IPAddress.Any, kofiko_port_number);

  }

  private void Update() {
    // update global eye positions
    FoveInterfaceBase.EyeRays rays = foveInterface.GetGazeRays();
    left_eye_vector = rays.left.direction;
    right_eye_vector = rays.right.direction;

    // always Serial send to Arduino
    double theta_L = (180.0 / 3.14159265) * -Math.Atan2( (double) left_eye_vector.x, (double) left_eye_vector.z);
    double rho_L = (180.0 / 3.14159265) * Math.Atan2((double) left_eye_vector.y, (double) Math.Sqrt(left_eye_vector.x*left_eye_vector.x + left_eye_vector.z*left_eye_vector.z));

    // Debug.Log(theta_L);
    // Debug.Log(rho_L);
    // converts stuff into bytes
    string dataString = theta_L.ToString() + "," + rho_L.ToString() + ";";
    // Debug.Log(dataString);
    arduinoPort.Write(dataString);
    /*
    // Debug.Log(dataString);
    byte[] send_buffer = Encoding.ASCII.GetBytes(dataString);
    //byte[] unicodeBytes = Encoding.Unicode.GetBytes(unicodeString);
    //byte[] asciiBytes = Encoding.Convert(Encoding.Unicode, Encoding.ASCII, unicodeBytes);
    udp_kofiko.Send(send_buffer, send_buffer.Length);
    // Debug.Log("done sending!");
    */
    if (triggered) {

      if (show_surface) {

        // float orientation_y = rotation_amount * (2 * UnityEngine.Random.value - 1);
        // float orientation_z = 90 + rotation_amount * (2 * UnityEngine.Random.value - 1);

        float orientation_x = -90 + rotation_amount * (2 * rotation_x - 1);
        float orientation_y = rotation_amount * (2 * rotation_y - 1);

        Quaternion rotation = Quaternion.identity;
        rotation.eulerAngles = new Vector3(orientation_x, orientation_y, 0) ;
        GameObject.Find ("surface").transform.localRotation = rotation ;

        // change position
        // float displacement_y = displacement_magnitude * (2 * UnityEngine.Random.value - 1);
        // float displacement_z = displacement_magnitude * (2 * UnityEngine.Random.value - 1);
        float displacement_y = displacement_magnitude * (2 * translation_y - 1);
        float displacement_x = displacement_magnitude * (2 * translation_x - 1);
        GameObject.Find ("surface").transform.localPosition = new Vector3(displacement_x, displacement_y, z_distance);

        // make appear
        // GameObject.Find ("fixation cross").transform.localScale = init_fixation_point_size*disappear_scale;
        GameObject.Find ("surface").transform.localScale = init_surface_size;

        triggered = false;
      } else {
        GameObject.Find ("surface").transform.localScale = init_surface_size*disappear_scale;
      }

      if (show_calibration) {
        // from angles: 1040, 540 to points ...
        float theta = 3.14159265f * ((translation_x - 540) / 540);
        float rho = 3.14159265f * (translation_y / 540);;
        float displacement_z = (float) (calibration_disparity * Math.Sin(rho) * Math.Cos(theta));
        float displacement_x = (float) (calibration_disparity * Math.Sin(rho) * Math.Sin(theta));
        float displacement_y = (float) (calibration_disparity * Math.Cos(rho));

        // calculate new coordinates
        GameObject.Find ("calibration point").transform.localPosition = new Vector3(displacement_x, displacement_y, displacement_z);
        GameObject.Find ("calibration point").transform.localScale = z_distance * 3.14159265f * (calibration_scale / 540) * (calibration_disparity / 7) * new Vector3(1.0f, 1.0f, 1.0f) ;
      } else {
        GameObject.Find ("calibration point").transform.localScale = init_surface_size*disappear_scale;
      }

      if (show_cross) {
        GameObject.Find ("fixation cross").transform.localScale = init_cross_size;
      } else {
        GameObject.Find ("fixation cross").transform.localScale = init_cross_size*disappear_scale;
      }
    }

   /*
    if (Input.GetKeyDown("space"))  {
      print("space key was pressed") ;

      if (show_surface) {
        // make disappear
        GameObject.Find ("surface").transform.localScale *= disappear_scale;
        // GameObject.Find ("fixation cross").transform.localScale /= disappear_scale;
      } else {
        // change orientation
        float orientation_y = rotation_amount * (2 * UnityEngine.Random.value - 1);
        float orientation_z = 90 + rotation_amount * (2 * UnityEngine.Random.value - 1);
        Quaternion rotation = Quaternion.identity;
        rotation.eulerAngles = new Vector3(0, orientation_y, orientation_z) ;
        GameObject.Find ("surface").transform.localRotation = rotation ;

        // change position
        float displacement_y = displacement_magnitude * (2 * UnityEngine.Random.value - 1);
        float displacement_z = displacement_magnitude * (2 * UnityEngine.Random.value - 1);
        GameObject.Find ("surface").transform.localPosition = new Vector3(x_distance, displacement_y, displacement_z);

        // make appear
        // GameObject.Find ("fixation cross").transform.localScale *= disappear_scale;
        GameObject.Find ("surface").transform.localScale /= disappear_scale;
      }
      show_surface = !(show_surface);
    }

    */
    return;
  }


}
