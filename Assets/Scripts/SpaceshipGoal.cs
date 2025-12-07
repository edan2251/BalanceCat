//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.SceneManagement;

//[DisallowMultipleComponent]
//public class SpaceshipGoal : MonoBehaviour
//{
//    [SerializeField] string playerTag = "Player";
//    [SerializeField] string mainSceneName = "Main";
//    [SerializeField] bool requireClear = true;

//    void OnTriggerEnter(Collider other)
//    {
//        if (!other.CompareTag(playerTag)) return;

//        if (!requireClear || ScoreZone.Cleared)
//        {
//            SceneManager.LoadScene(mainSceneName);
//        }
//    }
//}

