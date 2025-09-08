using UnityEngine;
using UnityEngine.SceneManagement;
public class CurrencyDugeonLoadButtion : MonoBehaviour
{
    [SerializeField] CurrencyDungeonType type;
    [SerializeField] GameObject prefab;
 
    public void OnClickButton()
    {
        SceneManager.LoadSceneAsync("CurrencyDungeonScene").completed += a =>
        {
            Instantiate(prefab);
        };
    }
}
