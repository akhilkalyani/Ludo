using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
    public class Tile : MonoBehaviour
    {
        public int tileNo;
        public bool isOccupied;
        public List<Token> occupiedTokensList = new List<Token>();
        public bool isSafeTile;
        [SerializeField] private Vector3 position;
        [SerializeField] private float xOffset;
        [SerializeField] private float yOffset;
        private Vector3 top;
        private Vector3 left;
        private Vector3 right;
        private Vector3 left_TopCorner;
        private Vector3 right_TopCorner;
        private Vector3 leftBottomCorner;
        private Vector3 rightBottomCorner;
        private void Start()
        {
            xOffset = 0.06f;
            yOffset = 0.06f;
            position = this.transform.position;
            top = new Vector3(position.x, position.y + yOffset, 0);
            left = new Vector3(position.x - xOffset, position.y, 0);
            right = new Vector3(position.x + xOffset, position.y, 0);
            left_TopCorner = new Vector3(position.x - xOffset, position.y + yOffset, 0);
            right_TopCorner = new Vector3(position.x + xOffset, position.y + yOffset, 0);
            leftBottomCorner = new Vector3(position.x - xOffset, position.y - yOffset, 0);
            rightBottomCorner = new Vector3(position.x + xOffset, position.y - yOffset, 0);
        }
        public void TokenArrived(Token _token)
        {
            if (isOccupied)
            {
                occupiedTokensList.Add(_token);
                ResizeToken();
            }
            else
            {
                occupiedTokensList.Add(_token);
                isOccupied = true;
            }

        }
        public Token GetExistingToken(Token token)
        {
            Token t = null;
            foreach (var _token in occupiedTokensList)
            {
                if (_token.tokenID != token.tokenID)
                {
                    t = _token;
                    break;
                }
            }
            if (t != null)
            {
                if (!this.isSafeTile)
                {
                    return t;
                }
            }
            return null;
        }
        public void RemoveTokenFromPath(Token _token)
        {
            bool isExist = false;
            foreach (var token in occupiedTokensList)
            {
                if (token.GetTokenIndex() == _token.GetTokenIndex())
                {
                    isExist = true;
                    break;
                }
            }
            if (isExist)
            {
                occupiedTokensList.Remove(_token);
                if (occupiedTokensList.Count == 0)
                {
                    isOccupied = false;
                }
            }
        }

        public void ResizeToken()
        {
            SetSize();
            if (occupiedTokensList.Count > 4)
            {
                switch (occupiedTokensList.Count)
                {
                    case 5:
                        break;
                    case 6:
                        break;
                    case 7:
                        break;
                    case 8:
                        break;
                    case 9:
                        break;
                    case 10:
                        break;
                    case 11:
                        break;
                    case 12:
                        break;
                    case 13:
                        break;
                    case 14:
                        break;
                    case 15:
                        break;
                    case 16:
                        break;
                }
            }
            else
            {
                SetPositionOfToken();
            }
        }

        private void SetPositionOfToken()
        {

            switch (occupiedTokensList.Count)
            {
                case 1:
                    occupiedTokensList[0].transform.position = position;
                    break;
                case 2:
                    //put token side by side by the x offset from the center...
                    occupiedTokensList[0].transform.position = left;
                    occupiedTokensList[1].transform.position = right;
                    break;
                case 3:
                    occupiedTokensList[0].transform.position = top;
                    occupiedTokensList[1].transform.position = leftBottomCorner;
                    occupiedTokensList[2].transform.position = rightBottomCorner;
                    break;
                case 4:
                    occupiedTokensList[0].transform.position = left_TopCorner;
                    occupiedTokensList[1].transform.position = right_TopCorner;
                    occupiedTokensList[2].transform.position = leftBottomCorner;
                    occupiedTokensList[3].transform.position = rightBottomCorner;
                    break;
            }
        }

        public void SetSize()
        {
            float size = 0;
            switch (occupiedTokensList.Count)
            {
                case 1:
                    size = 1;
                    break;
                case 2:
                    size = 0.5f;
                    break;
                case 3:
                    size = 0.35f;
                    break;
                case 4:
                    size = 0.25f;
                    break;
            }
            foreach (var _token in occupiedTokensList)
            {
                _token.transform.localScale = new Vector3(size, size, size);
            }
        }
    }
