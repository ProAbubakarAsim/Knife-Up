using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneChange : MonoBehaviour
{
    public void ChangeScene(int sceneNum)
    {
        Application.LoadLevel(sceneNum);
    }
}
