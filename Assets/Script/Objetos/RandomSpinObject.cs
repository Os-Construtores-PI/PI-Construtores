using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RandomSpinObject : BaseRenderedGameObject
{
    [SerializeField]
    private ActTransition _transitionListener;

    [SerializeField]
    private List<LevelPath> _paths = new();

    [SerializeField]
    private int _spins = 4;

    [SerializeField]
    private float _rotationDuration = 5f;

    [SerializeField]
    private float _intervalDuration = 2f;

    private LevelPathType lastPath;

    public override void Start()
    {
        if (_transitionListener != null)
        {
            _transitionListener.Transition.AddListener(RotationAnimation);
        }
    }

    public void RotationAnimation()
    {
        int currentSlot = DataDirector.Instance.GetCurrentSlot();
        LevelPathType lastPathData = DataDirector.Instance.GetLastPath(
            currentSlot,
            SceneManager.GetActiveScene().name
        );

        if (lastPathData != default)
        {
            lastPath = lastPathData;
        }

        if (_paths.Count <= 1)
        {
            Debug.LogError("Not enough paths to avoid repetition.");
            return;
        }

        int randomIndex = Random.Range(0, _paths.Count);
        while (_paths[randomIndex].PathType == lastPath)
        {
            randomIndex = Random.Range(0, _paths.Count);
        }

        LevelPath randomPath = _paths[randomIndex];
        lastPath = randomPath.PathType;
        DataDirector.Instance.SaveLastPath(
            DataDirector.Instance.GetCurrentSlot(),
            randomPath.PathType
        );

        float pathRotation = randomPath.Rotation;
        Vector3 angleVector = new Vector3(0, 360 * _spins + pathRotation, 0);

        Sequence animationSequence = DOTween.Sequence();
        animationSequence.AppendInterval(3f);
        animationSequence.Append(
            transform
                .DORotate(angleVector, _rotationDuration, RotateMode.FastBeyond360)
                .SetEase(Ease.OutExpo)
        );
        animationSequence.Play();
    }
}
