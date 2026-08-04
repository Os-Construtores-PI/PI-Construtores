using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RankingEntriesContainer : MonoBehaviour
{
  [SerializeField]
  private ManualSingleSpawner entriesSpawner;

  private List<SavedLevelFinish> _levelFinishes = new();

  public void Start()
  {
    DataDirector dataDirector = DataDirector.Instance;
    if (!dataDirector)
      return;

    _levelFinishes = dataDirector
      .GetLevelFinishes(dataDirector.GetCurrentSlot(), "Fase0")
      .OrderByDescending(finish => finish.Score)
      .ThenBy(finish => finish.Time)
      .ToList();

    entriesSpawner.SetAmountPool(_levelFinishes.Count);
    entriesSpawner.FinishedInstancing.AddListener(OnEntriesInstanced);
    entriesSpawner.enabled = true;
  }

  private void OnEntriesInstanced(List<GameObject> pooledEntries)
  {
    entriesSpawner.FinishedInstancing.RemoveListener(OnEntriesInstanced);

    int count = Mathf.Min(pooledEntries.Count, _levelFinishes.Count);
    for (int i = 0; i < count; i++)
    {
      GameObject entryObject = pooledEntries[i];
      if (entryObject == null)
        continue;

      entryObject.SetActive(true);

      if (entryObject.TryGetComponent(out RankingEntry rankingEntry))
      {
        SavedLevelFinish finish = _levelFinishes[i];
        rankingEntry.SetData(i + 1, finish.FinishUUID, finish.Score, finish.Time);
      }
    }
  }
}
