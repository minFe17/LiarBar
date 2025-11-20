using System.Collections.Generic;
using UnityEngine;

public class CharacterList : MonoBehaviour
{
    [SerializeField] List<GameObject> _characterList;

    int _currentIndex = 0;

    void Start()
    {
        for (int i = 0; i < _characterList.Count; i++)
        {
            if (i == _currentIndex)
                _characterList[i].SetActive(true);
            else
                _characterList[i].SetActive(false);
        }
    }

    public int ChangeCharacter(int direction)
    {
        _characterList[_currentIndex].SetActive(false);
        _currentIndex += direction;

        if (_currentIndex < 0)
            _currentIndex = _characterList.Count - 1;
        else if (_currentIndex >= _characterList.Count)
            _currentIndex = 0;
        _characterList[_currentIndex].SetActive(true);

        return _currentIndex;
    }

    public void ShowCharacter(int index)
    {
        for (int i = 0; i < _characterList.Count; i++)
            _characterList[i].SetActive(i == index);

        _currentIndex = index;
    }
}