using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpineAnalyzer
{
    public class Position
    {
        private float _X;
        private float _Y;
        private float _Z;

        public float X
        {
            get { return _X; }
            set { _X = value; }
        }
        public float Y
        {
            get { return _Y; }
            set { _Y = value; }
        }
        public float Z
        {
            get { return _Z; }
            set { _Z = value; }
        }

        public Position(float X, float Y, float Z)
        {
            this._X = X;
            this._Y = Y;
            this._Z = Z;
        }

    }
}
