using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public class PathManager : MonoBehaviour
    {
        [SerializeField] private List<Tile> outerPath;
        [SerializeField] private List<Tile> redInnerPath;
        [SerializeField] private List<Tile> greenInnerPath;
        [SerializeField] private List<Tile> yellowInnerPath;
        [SerializeField] private List<Tile> blueInnerPath;
        [SerializeField] private Tile redStartPos;
        [SerializeField] private Tile greenStartPos;
        [SerializeField] private Tile yellowStartPos;
        [SerializeField] private Tile blueStartPos;
        [SerializeField] private int boardLeftSideStartingIndex = 26;
        public void CreatePath()
        {
            int redStartIndex = outerPath.IndexOf(redStartPos);
            int greenStartIndex = outerPath.IndexOf(greenStartPos);
            int yellowStartIndex = outerPath.IndexOf(yellowStartPos);
            int blueStartIndex = outerPath.IndexOf(blueStartPos);
            SetTokenPath(redStartIndex, 1, redInnerPath);
            SetTokenPath(greenStartIndex, 2, greenInnerPath);
            SetTokenPath(yellowStartIndex, 3, yellowInnerPath);
            SetTokenPath(blueStartIndex, 4, blueInnerPath);
        }
        public int GetBoardLeftSideStartingIndex()
        {
            return boardLeftSideStartingIndex;
        }
        private void SetTokenPath(int startIndex, int tokenType, List<Tile> innerPath)
        {
            List<Transform> tempList = new List<Transform>();
            for (int i = 0; i < outerPath.Count - 1; i++)
            {
                int index = startIndex + i;
                index %= outerPath.Count;
                foreach (var token in GameManager.instance.players[tokenType - 1].tokens)
                {
                    token.finalPath.Add(outerPath[index]);
                }

            }
            foreach (var path in innerPath)
            {
                foreach (var token in GameManager.instance.players[tokenType - 1].tokens)
                {
                    token.finalPath.Add(path);
                }
            }
        }
        public List<Tile> GetOuterPath()
        {
            return outerPath;
        }
    }
