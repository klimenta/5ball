// Copyright(c) 2016, Kliment Andreev
// All rights reserved.
// Simplified BSD License, see LICENSE file
// https://github.com/klimenta/5ball/blob/master/LICENSE
//======================================================
// Made with Visual Studio 2015, Community Edition
//======================================================
// Try to match 5 or more balls in any direction to score
// Drag & drop to move a ball inside the grid

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Diagnostics;
using System.Windows.Forms;

namespace _5ball
{
    public partial class Form1 : Form
    {
        //Grid elements 9 x 9
        public const int cintGridElements = 9;
        //Min amount of balls in any direction in order to scrore
        const int cintMinBalls = 5;
        //How many balls we throw on the grid after each turn
        const int cintThrowBalls = 3;
        //These are the lines surrounding each grid element
        const int cintGridBorders = cintGridElements + 1;
        //The grid element starts 32 pixels from left
        const int cintLeftPadding = 32;
        //The grid element starts 36 pixels from top
        const int cintTopPadding = 36;
        //Grid element width
        const int cintPictureSizeWidth = 50;
        //Grid element height
        const int cintPictureSizeHeight = 50;
        //Gap between each grid element, filled by a horizontal or vertical line
        const int cintGap = 2;
        //String format for the score
        const string strScoreFormat = "00000";
        //Some temp variables and counters
        int i, j;
        //Score variable
        public static int intScore = 0;
        //Total number of grid elements 
        public static int intAvailableGridElements = cintGridElements * cintGridElements;
        //Each ball has a unique tag depending on what grid element it occupies. 
        //The tag starts with 0 (top most left element) and ends with 80 (bottom right element)
        //We have a total of 81 elements, because 9 x 9 = 81 (our grid size)
        //Source tag is the element that is being moved, dest tag is the element where the ball will end when 
        //dragged and dropped
        public static int intSourceTag, intDestTag;
        //These are the coordinates of the elements that are being moved
        //(0,0) is the top most left element, (1,0) is the element right next to it
        //(1,0) is the element below the top most left element and (8,8) is the bottom right element
        public static int intSourceX, intDestX;
        public static int intSourceY, intDestY;
        //A struct to return coordinates. Easier to deal that returning two integers from a method
        public struct XYCoordinates
        {
            public int intX;
            public int intY;
        }
        //Dictionary for the balls, each shape coresponds with a number
        public static Dictionary<int, Bitmap> dicBalls = new Dictionary<int, Bitmap>();
        //Array of picture elements
        public static PictureBox[,] arrPictureBox = new PictureBox[cintGridElements, cintGridElements];
        //Array of vertical grid pictures. The yellow bars between each element are pictures.
        public static PictureBox[] arrVerticalGrid = new PictureBox[cintGridBorders];
        //Array of horizontal grid pictures. The yellow bars between each element are pictures.
        public static PictureBox[] arrHorizontalGrid = new PictureBox[cintGridBorders];
        //Array of the grid elements
        public static int[,] arrGridElements = new int[cintGridElements, cintGridElements];
        //Random number generator
        public static Random rnd = new Random();
        //A struct to return the starting position and maximum balls in any direction
        public struct structReturnPositionAndMaxBalls
        {
            public int intPosition;
            public int intMaxBalls;
        };
        //These are all possible directions: 
        // Row, Column, LowerLeft-UpperRight, UpperRight-LowerLeft, UpperLeft-LowerRight and LowerRight-UpperLeft
        public static List<string> listCombinations =
            new List<string>(new string[] { "R", "C", "LLUR", "URLL", "ULLR", "LRUL" });
        //Temp struct
        public static structReturnPositionAndMaxBalls sTemp;
        //Main form
        public Form1()
        {
            InitializeComponent();
            AllowDrop = false;
            //This is how the different balls are referred in the dictionary
            // 1 = Green, 2 = Yellow, 3 = Blue, 4 = Purple, 5 = Orange
            dicBalls.Add(1, Properties.Resources.green);
            dicBalls.Add(2, Properties.Resources.yellow);
            dicBalls.Add(3, Properties.Resources.blue);
            dicBalls.Add(4, Properties.Resources.purple);
            dicBalls.Add(5, Properties.Resources.orange);
            //Initialize the grid elements array and the pucture boxes
            for (i = 0; i < cintGridElements; i++)
            {
                for (j = 0; j < cintGridElements; j++)
                {
                    arrPictureBox[i, j] = new PictureBox();
                    arrPictureBox[i, j].Parent = this;
                    arrPictureBox[i, j].Top = cintTopPadding + (cintPictureSizeHeight + cintGap) * j;
                    arrPictureBox[i, j].Left = cintLeftPadding + (cintPictureSizeWidth + cintGap) * i;
                    arrPictureBox[i, j].Width = cintPictureSizeWidth;
                    arrPictureBox[i, j].Height = cintPictureSizeHeight;
                    arrPictureBox[i, j].MouseDown += pictureBox_MouseDown;
                    arrPictureBox[i, j].AllowDrop = true;
                    arrPictureBox[i, j].DragEnter += pictureBox_DragEnter;
                    arrPictureBox[i, j].DragDrop += pictureBox_DragDrop;
                    //Assign a tag to each grid element (from 0 to 80)
                    arrPictureBox[i, j].Tag = (i + j) + (j * (cintGridElements - 1));
                    //Initialize the grid with empty grid elements
                    arrGridElements[i, j] = 0;
                }
            }
            //Put the vertical and horizontal bars on the form
            for (i = 0; i < cintGridBorders; i++)
            {
                arrHorizontalGrid[i] = new PictureBox();
                arrHorizontalGrid[i].Parent = this;
                arrHorizontalGrid[i].Top = cintTopPadding + (cintPictureSizeHeight + cintGap) * i - cintGap;
                arrHorizontalGrid[i].Left = cintLeftPadding - cintGap;
                arrHorizontalGrid[i].Width =
                    cintPictureSizeWidth * cintGridElements + (cintGridElements - 1) * cintGap + 2 * cintGap;
                arrHorizontalGrid[i].Height = cintGap;
                arrHorizontalGrid[i].Image = Properties.Resources.HorizontalGrid;
                arrHorizontalGrid[i].AllowDrop = false;

                arrVerticalGrid[i] = new PictureBox();
                arrVerticalGrid[i].Parent = this;
                arrVerticalGrid[i].Top = cintTopPadding - cintGap;
                arrVerticalGrid[i].Left = cintLeftPadding + (cintPictureSizeWidth + cintGap) * i - cintGap;
                arrVerticalGrid[i].Width = cintGap;
                arrVerticalGrid[i].Height =
                    cintPictureSizeHeight * cintGridElements + (cintGridElements - 1) * cintGap + 2 * cintGap;
                arrVerticalGrid[i].Image = Properties.Resources.VerticalGrid;
                arrVerticalGrid[i].AllowDrop = false;
            }
            //Throw three random balls on the grid
            ThrowThreeRandomBalls();
            //Update the score
            UpdateScoreLabel();
        }
        //Find balls that can be destroyed
        //Input:  Start position, color (e.g 1, 2, etc..) and a direction parameter "R" or "C" or "URLL" etc...
        //Output: a struct with the position and number of balls if 5 or more balls were found
        //        If not, output is (-1, -1)
        public static structReturnPositionAndMaxBalls FindBalls(int intRowOrCol, int intColor, string strRowOrCol)
        {
            List<int> listTempList1 = new List<int>();
            List<int> listTempList2 = new List<int>();
            structReturnPositionAndMaxBalls structTemp;
            //Assume that there are 9 balls that can be destroyed
            int intBallCounter = cintGridElements; int intFirstPosition = 0;
            do
            {
                for (int z = 0; z < intBallCounter; z++)
                {
                    listTempList2.Add(intColor);
                }
                //Check all directions
                //Return a list of possible hits
                switch (strRowOrCol)
                {
                    case "R":
                        arrGridElements.CopyRow(listTempList1, intRowOrCol, 0);
                        break;
                    case "C":
                        arrGridElements.CopyCol(listTempList1, 0, intRowOrCol);
                        break;
                    case "LLUR":
                        arrGridElements.CopyStartsULEndsLLUR(listTempList1, intRowOrCol, 0);
                        break;
                    case "URLL":
                        arrGridElements.CopyStartsURLLEndsLR(listTempList1, intRowOrCol, 8);
                        break;
                    case "ULLR":
                        arrGridElements.CopyStartsULLREndsLL(listTempList1, intRowOrCol, 0);
                        break;
                    case "LRUL":
                        arrGridElements.CopyStartsUREndsLRUL(listTempList1, intRowOrCol, 8);
                        break;
                }
                //Convert lists to arrays
                int[] arrTempArray1 = listTempList1.ToArray();
                int[] arrTempArray2 = listTempList2.ToArray();
                intFirstPosition = arrTempArray1.StartingIndex(arrTempArray2);
                if (intFirstPosition != -1)
                {
                    structTemp.intPosition = intFirstPosition;
                    structTemp.intMaxBalls = intBallCounter;
                    return structTemp;
                }
                listTempList2.Clear();
                listTempList1.Clear();
                intBallCounter--;
            } while (intBallCounter > (cintGridElements - cintMinBalls));
            structTemp.intPosition = -1;
            structTemp.intMaxBalls = -1;
            return structTemp;
        }
        //Destroy the balls that were found
        //in any direction
        //Remove the images from picure boxes and put 0 in the array grid
        public static void DestroyBalls(int intRow, string strRowOrCol, structReturnPositionAndMaxBalls structTemp, int intColor)
        {
            int intCounterRow = 0;
            int intCounterCol = cintGridElements - 1;
            switch (strRowOrCol)
            {
                case "R":
                    for (int x = 0 + structTemp.intPosition; x < (structTemp.intMaxBalls + structTemp.intPosition); x++)
                    {
                        arrPictureBox[intRow, x].Image = null;
                        arrGridElements[intRow, x] = 0;
                    }
                    break;
                case "C":
                    for (int x = 0 + structTemp.intPosition; x < (structTemp.intMaxBalls + structTemp.intPosition); x++)
                    {
                        arrPictureBox[x, intRow].Image = null;
                        arrGridElements[x, intRow] = 0;
                    }
                    break;
                case "LLUR":
                    for (int x = intRow - structTemp.intPosition; x > (intRow - structTemp.intPosition - structTemp.intMaxBalls); x--)
                    {
                        arrPictureBox[x, intCounterRow + structTemp.intPosition].Image = null;
                        arrGridElements[x, intCounterRow + structTemp.intPosition] = 0;
                        intCounterRow++;
                    }
                    break;
                case "URLL":
                    for (int x = intRow + structTemp.intPosition; x < (intRow + structTemp.intPosition + structTemp.intMaxBalls); x++)
                    {
                        arrPictureBox[x, intCounterCol - structTemp.intPosition].Image = null;
                        arrGridElements[x, intCounterCol - structTemp.intPosition] = 0;
                        intCounterCol--;
                    }
                    break;
                case "ULLR":
                    for (int x = intRow + structTemp.intPosition; x < (intRow + structTemp.intPosition + structTemp.intMaxBalls); x++)
                    {
                        arrPictureBox[x, intCounterRow + structTemp.intPosition].Image = null;
                        arrGridElements[x, intCounterRow + structTemp.intPosition] = 0;
                        intCounterRow++;
                    }
                    break;
                case "LRUL":
                    for (int x = intRow - structTemp.intPosition; x > (intRow - structTemp.intPosition - structTemp.intMaxBalls); x--)
                    {
                        arrPictureBox[x, intCounterCol - structTemp.intPosition].Image = null;
                        arrGridElements[x, intCounterCol - structTemp.intPosition] = 0;
                        intCounterCol--;
                    }
                    break;
            }
            //Increase the score, 10 points for each ball destroyed
            intScore = intScore + (structTemp.intMaxBalls * 10);            
        }
        //Updates the score
        public void UpdateScoreLabel()
        {            
            
            lblScore.Text = intScore.ToString(strScoreFormat);

        }
        //When user drops the ball to a new position
        private void pictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                try
                {
                    var dragImage = (Bitmap)(sender as PictureBox).Image;
                    intSourceTag = Convert.ToInt32((sender as PictureBox).Tag);
                    IntPtr icon = dragImage.GetHicon();
                    Cursor.Current = new Cursor(icon);
                    if (DoDragDrop((sender as PictureBox).Image, DragDropEffects.Copy) == DragDropEffects.Copy)
                    {
                        //If it's the same position -> return
                        if (intSourceTag == intDestTag) return;
                        //If the destination position is already taken -> return
                        if (arrGridElements[intDestX, intDestY] > 0)
                        {
                            return;
                        }
                        //If everything OK -> update the picture box and the array grid
                        else {
                            arrGridElements[intDestX, intDestY] = arrGridElements[intSourceX, intSourceY];
                            arrGridElements[intSourceX, intSourceY] = 0;
                            (sender as PictureBox).Image = null;
                        }
                    }
                    DestroyIcon(icon);
                    //Check if 5 or more balls are present in any directions
                    CheckFor5Elements();
                    //Throw three new random balls
                    ThrowThreeRandomBalls();
                    //Check again
                    CheckFor5Elements();
                    //Sometimes the random balls can close a gap and score
                    //Update the score in that case
                    UpdateScoreLabel();
                    //How many available grid elements are there
                    intAvailableGridElements = Form1.CountEmptyGridElements();
                    //If none available it means the game is over
                    if (intAvailableGridElements == 0)
                    {
                        GameOver();
                        intScore = 0;
                        UpdateScoreLabel();
                        return;
                    }
                }
                catch (Exception)
                {
                    //Just ignore
                }
            }
        }

        protected override void OnGiveFeedback(GiveFeedbackEventArgs e)
        {
            e.UseDefaultCursors = false;
        }
        //User begins dragging a ball
        void pictureBox_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(Bitmap))) e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }

        void pictureBox_DragDrop(object sender, DragEventArgs e)
        {
            var bmp = (Bitmap)e.Data.GetData(typeof(Bitmap));
            intDestTag = Convert.ToInt32((sender as PictureBox).Tag);
            //Return if drag & drop @ the same grid element
            if (intSourceTag == intDestTag) return;
            intSourceX = intSourceTag % cintGridElements;
            intSourceY = intSourceTag / cintGridElements;
            intDestX = intDestTag % cintGridElements;
            intDestY = intDestTag / cintGridElements;
            if (arrGridElements[intDestX, intDestY] > 0) return;
            (sender as PictureBox).Image = bmp;
        }
        //Check if there are 5 or more available elements to be destroyed
        //If found, destroy them
        public static void CheckFor5Elements()
        {
            foreach (string strRowOrCol in listCombinations)
            {
                foreach (KeyValuePair<int, Bitmap> intEntry in dicBalls)
                {
                    for (int intDirection = 0; intDirection < cintGridElements; intDirection++)
                    {
                        sTemp = FindBalls(intDirection, intEntry.Key, strRowOrCol);
                        if (sTemp.intPosition != -1)
                        {
                            DestroyBalls(intDirection, strRowOrCol, sTemp, intEntry.Key);
                        }
                    }
                }
            }
        }
        //Throws three balls on the grid
        public static void ThrowThreeRandomBalls()
        {
            int intEmptyBalls, intThreeOrLessCounter, intThreeOrLessEmptyBalls, intRandomBall;
            XYCoordinates structEmptyBallCoordinates;
            //Checks if the game is over after each throw
            intEmptyBalls = CountEmptyGridElements();
            if (intEmptyBalls == 0)
            {
                GameOver();
                return;
            }
            //If there are less than three grid elements available
            //throws two or one ball(s)
            if (intEmptyBalls > cintThrowBalls) intThreeOrLessEmptyBalls = cintThrowBalls;
                else intThreeOrLessEmptyBalls = intEmptyBalls;
            for (intThreeOrLessCounter = 0; intThreeOrLessCounter < intThreeOrLessEmptyBalls; intThreeOrLessCounter++)
            {                
                intRandomBall = rnd.Next(1, dicBalls.Count + 1);
                structEmptyBallCoordinates = GetRandomAvailableGridElement();
                arrPictureBox[structEmptyBallCoordinates.intX, structEmptyBallCoordinates.intY].Image = dicBalls[intRandomBall];
                arrGridElements[structEmptyBallCoordinates.intX, structEmptyBallCoordinates.intY] = intRandomBall;
            }
        }
        //Initialize the grid and picture boxes, ready for a new game
        public static void StartNewGame()
        {
            for (int i = 0; i < cintGridElements; i++)
            {
                for (int j = 0; j < cintGridElements; j++)
                {
                    arrPictureBox[i, j].Image = null;
                    arrGridElements[i, j] = 0;
                }
            }                      
            ThrowThreeRandomBalls();
        }
        //Show the game over message and ask for another game
        public static void GameOver()
        {
            if (MessageBox.Show("Do you want another game?", "!!! GAME OVER !!!",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
            {
                Application.Exit();
            }
            else
            {
                StartNewGame();
            }
        }
        //Count how many 0s are there, meaning how many available grid elements we have
        public static int CountEmptyGridElements()
        {
            int i, j, intEmptyCounter;
            intEmptyCounter = 0;
            for (i = 0; i < cintGridElements; i++)
            {
                for (j = 0; j < cintGridElements; j++)
                {
                    if (arrGridElements[i, j] == 0) intEmptyCounter++;
                }
            }
            return intEmptyCounter;
        }
        //When the forms start, define the links for the bottom right link
        private void Form1_Load(object sender, EventArgs e)
        {
            LinkLabel.Link link = new LinkLabel.Link();
            link.LinkData = "http://blog.iandreev.com/";
            linklblBlog.Links.Add(link);
        }
        //Go to the link URL if clicked
        private void linklblBlog_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(e.Link.LinkData as string);
        }
        //When the main form closes, ask the user if he wants to exit
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (MessageBox.Show("Click Yes to Exit, No to stay...", "Are you sure you want to exit?",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
            {
                e.Cancel = true;
            }
        }
        //Gets a random available element, returns a struct with (x,y) coordinates
        public static XYCoordinates GetRandomAvailableGridElement()
        {
            XYCoordinates XY;
            int i, j;
            do
            {
                i = rnd.Next(0, cintGridElements);
                j = rnd.Next(0, cintGridElements);
            } while (arrGridElements[i, j] != 0);
            XY.intX = i;
            XY.intY = j;
            return XY;
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        extern static bool DestroyIcon(IntPtr handle);
    }
    //This is a class to extend the array methods
    //Looking to see if an array is part of another array
    //that's how we are looking for same balls in any direction
    public static class ArrayExtensions
    {
        private static bool IsSubArrayEqual(int[] x, int[] y, int start)
        {
            for (int i = 0; i < y.Length; i++)
            {
                if (x[start++] != y[i]) return false;
            }
            return true;
        }

        public static int StartingIndex(this int[] x, int[] y)
        {
            int max = 1 + x.Length - y.Length;
            for (int i = 0; i < max; i++)
            {
                if (IsSubArrayEqual(x, y, i)) return i;
            }
            return -1;
        }

        public static void CopyCol(this int[,] x, List<int> y, int row, int col)
        {
            int i;
            for (i = 0; i < Form1.cintGridElements; i++)
            {
                y.Add(x[row + i, col]);
            }
        }

        public static void CopyRow(this int[,] x, List<int> y, int row, int col)
        {
            int i;
            for (i = 0; i < Form1.cintGridElements; i++)
            {
                y.Add(x[row, col + i]);
            }
        }

        public static void CopyStartsULEndsLLUR(this int[,] x, List<int> y, int row, int col)
        {
            int i, temp;
            for (i = 0; i < Form1.cintGridElements; i++)
            {
                temp = row - i;
                if (temp < 0)
                    y.Add(0);
                else
                    y.Add(x[row - i, col + i]);
            }
        }

        public static void CopyStartsURLLEndsLR(this int[,] x, List<int> y, int row, int col)
        {
            int i, temp;
            for (i = 0; i < Form1.cintGridElements; i++)
            {
                temp = row + i;
                if (temp > (Form1.cintGridElements - 1))
                    y.Add(0);
                else
                    y.Add(x[row + i, col - i]);
            }
        }

        public static void CopyStartsULLREndsLL(this int[,] x, List<int> y, int row, int col)
        {
            int i, temp;
            for (i = 0; i < Form1.cintGridElements; i++)
            {
                temp = row + i;
                if (temp > (Form1.cintGridElements - 1))
                    y.Add(0);
                else
                    y.Add(x[row + i, col + i]);
            }
        }

        public static void CopyStartsUREndsLRUL(this int[,] x, List<int> y, int row, int col)
        {
            int i, temp;
            for (i = 0; i < Form1.cintGridElements; i++)
            {
                temp = row - i;
                if (temp < 0)
                    y.Add(0);
                else
                    y.Add(x[row - i, col - i]);
            }
        }
    }
}
