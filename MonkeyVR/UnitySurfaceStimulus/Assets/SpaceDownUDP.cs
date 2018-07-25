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
  int port_number = 12345;
  static UdpClient udp;
  Thread thread;
  static readonly object lockObject = new object();
  string returnData = "";

  bool triggered = false;


  bool show_surface = true;
  float disappear_scale = 0.0001F;
  float rotation_amount = 25.0F; // in degrees
  float x_distance = 7.0F;
  float displacement_magnitude = 1.0F; // the width of the square is 2.5 to begin with

  Vector3 init_surface_size;
  Vector3 init_fixation_point_size;

  float rotation_y = 0.5f;
  float rotation_z = 0.5f;
  float translation_y = 0.5f;
  float translation_z = 0.5f;


  private void ThreadMethod() {
    print ("This loop is running!");

    udp = new UdpClient(12345);
    while (true) {
      IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, port_number);

      byte[] receiveBytes = udp.Receive(ref RemoteIpEndPoint);

      lock (lockObject)
      {
        returnData = Encoding.ASCII.GetString(receiveBytes);
        Debug.Log(returnData);
        string[] words = returnData.Split(',');

        if (bool.Parse(words[0])) { // surface is to be turned on
          rotation_y = float.Parse(words[1]);
          rotation_z = float.Parse(words[2]);
          translation_y = float.Parse(words[3]);
          translation_z = float.Parse(words[4]);

          // assume surface is shown
          show_surface = true;
          triggered = true;
        } else {
          // turn it off!!!
          show_surface = false;
          triggered = true;
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

  }

  private void Update() {
    if (triggered) {

      if (show_surface) {
        // make disappear really fast
        GameObject.Find ("surface").transform.localScale = init_surface_size*disappear_scale;
        GameObject.Find ("fixation point").transform.localScale = init_fixation_point_size;
      }
      else {

        // float orientation_y = rotation_amount * (2 * UnityEngine.Random.value - 1);
        // float orientation_z = 90 + rotation_amount * (2 * UnityEngine.Random.value - 1);
        float orientation_y = rotation_amount * (2 * rotation_y - 1);
        float orientation_z = 90 + rotation_amount * (2 * rotation_z - 1);

        Quaternion rotation = Quaternion.identity;
        rotation.eulerAngles = new Vector3(0, orientation_y, orientation_z) ;
        GameObject.Find ("surface").transform.localRotation = rotation ;

        // change position
        // float displacement_y = displacement_magnitude * (2 * UnityEngine.Random.value - 1);
        // float displacement_z = displacement_magnitude * (2 * UnityEngine.Random.value - 1);
        float displacement_y = displacement_magnitude * (2 * translation_y - 1);
        float displacement_z = displacement_magnitude * (2 * translation_z - 1);
        GameObject.Find ("surface").transform.localPosition = new Vector3(x_distance, displacement_y, displacement_z);

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
