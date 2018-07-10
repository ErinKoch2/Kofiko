using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class SpaceDown : MonoBehaviour {
  bool show_surface = false;
  float disappear_scale = 0.0001F;
  float rotation_amount = 25.0F; // in degrees
  float x_distance = 7.0F;
  float displacement_magnitude = 2.5F; // the width of the square is 2.5 to begin with

  private void Start() {
    if (show_surface) {
      GameObject.Find ("fixation point").transform.localScale *= disappear_scale;
    } else {
      GameObject.Find ("surface").transform.localScale *= disappear_scale;
    }

  }

  private void Update() {
    if (Input.GetKeyDown("space")) {
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
    return;
  }
}
