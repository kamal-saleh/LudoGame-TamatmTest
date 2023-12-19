using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Linq;

public class AddressablesLoader : MonoBehaviour
{
    [SerializeField] private AssetLabelReference gameObjectsLabelReference;
    [SerializeField] private GameManager gameManager;

    public void LoadAddressables()
    {
        Addressables.LoadAssetsAsync<GameObject>(gameObjectsLabelReference, (go) =>
        {}).Completed += (gos) => {
            gameManager.SetAnimationGameObjects(gos.Result.OrderBy(x => x.name).ToList());
        };
    }
}