public class Randomizer
{
  int lastNumb = default;
  float lastNumbf = default;

  public int PositiveIntRandom(int range)
  {
    int sortNumb = UnityEngine.Random.Range(0, range);
    while (sortNumb == lastNumb)
    {
      sortNumb = UnityEngine.Random.Range(0, range);
    }
    lastNumb = sortNumb;
    return sortNumb;
  }

  public float PositiveFloatRandom(int range)
  {
    float sortNumb = UnityEngine.Random.Range(0, range);
    while (sortNumb == lastNumbf)
    {
      sortNumb = UnityEngine.Random.Range(0, range);
    }
    lastNumbf = sortNumb;
    return sortNumb;
  }
}
