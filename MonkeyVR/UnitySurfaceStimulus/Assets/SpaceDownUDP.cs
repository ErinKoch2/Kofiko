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

public class SpaceDownUDP : MonoBehaviour {
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


  bool show_surface = true;
  float disappear_scale = 0.0001F;
  float rotation_amount = 25.0F; // in degrees
  // float x_distance = 7.0F;
  float z_distance = 7.0F;
  float displacement_magnitude = 1.0F; // the width of the square is 2.5 to begin with

  Vector3 init_surface_size;
  Vector3 init_fixation_point_size;

  float rotation_y = 0.5f;
  // float rotation_z = 0.5f;
  float rotation_x = 0.5f;
  float translation_x = 0.5f;
  float translation_y = 0.5f;
  // float translation_z = 0.5f;

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

        if (words[0].Equals("surfaceOn")) {
          rotation_x = float.Parse(words[1]);
          rotation_y = float.Parse(words[2]);
          translation_x = float.Parse(words[3]);
          translation_y = float.Parse(words[4]);

          // assume surface is shown
          show_surface = true;
          triggered = true;
        } else if (words[0].Equals("surfaceOff")) {
          // turn it off!!!
          show_surface = false;
          triggered = true;
        } else if (words[0].Equals("getAnalog")) {
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


        }

      }
    }
  }

  private void Start() {
    thread = new Thread(new ThreadStart(ThreadMethod));
    print ("starting loop");
    thread.Start();


    init_surface_size = GameObject.Find ("surface").transform.localScale;
    init_fixation_point_size = GameObject.Find ("fixation point").transform.localScale;

    if (show_surface) {
      GameObject.Find ("fixation point").transform.localScale = init_fixation_point_size*disappear_scale;
    } else {
      GameObject.Find ("surface").transform.localScale = init_surface_size*disappear_scale;
    }
    endPoint = new IPEndPoint(IPAddress.Any, kofiko_port_number);

  }

  private void Update() {
    // update global eye positions
    FoveInterfaceBase.EyeRays rays = foveInterface.GetGazeRays();
    left_eye_vector = rays.left.direction;
    right_eye_vector = rays.right.direction;

    if (triggered) {

      if (show_surface) {
        // make disappear really fast
        GameObject.Find ("surface").transform.localScale = init_surface_size*disappear_scale;
        GameObject.Find ("fixation point").transform.localScale = init_fixation_point_size;
      }
      else {

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
        GameObject.Find ("fixation point").transform.localScale = init_fixation_point_size*disappear_scale;
        GameObject.Find ("surface").transform.localScale = init_surface_size;

        show_surface = true;
        triggered = false;
      }
    }

   /*
    if (Input.GetKeyDown("space"))  {
      print("space key was pressed") ;

      if (show_surface) {
        // make disappear
        GameObject.Find ("surface").transform.localScale *= disappear_scale;
        GameObject.Find ("fixation point").transform.localScale /= disappear_scale;
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
        GameObject.Find ("fixation point").transform.localScale *= disappear_scale;
        GameObject.Find ("surface").transform.localScale /= disappear_scale;
      }
      show_surface = !(show_surface);
    }

    */
    return;
  }


}
