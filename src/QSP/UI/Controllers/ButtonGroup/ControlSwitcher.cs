﻿using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace QSP.UI.Controllers.ButtonGroup
{
    public class ControlSwitcher
    {
        private BtnControlPair[] _pairings;
        private bool _subscribed;

        public IEnumerable<BtnControlPair> Pairings
        {
            get { return _pairings; }
        }

        public bool Subscribed
        {
            get
            {
                return _subscribed;
            }

            set
            {
                if (_subscribed && value == false)
                {
                    foreach (var i in _pairings)
                    {
                        i.Button.Click -= ShowControl;
                    }
                    _subscribed = false;
                }
                else if (_subscribed == false && value)
                {
                    foreach (var i in _pairings)
                    {
                        i.Button.Click += ShowControl;
                    }
                    _subscribed = true;
                }
            }
        }

        public ControlSwitcher(params BtnControlPair[] pairings)
        {
            this._pairings = pairings;
            _subscribed = false;
        }

        private void ShowControl(object sender, EventArgs e)
        {
            foreach (var i in _pairings)
            {
                i.Control.Visible = (i.Button == sender);
            }
        }

        public class BtnControlPair
        {
            public Button Button { get; private set; }
            public UserControl Control { get; private set; }

            public BtnControlPair(Button Button, UserControl Control)
            {
                this.Button = Button;
                this.Control = Control;
            }
        }
    }
}
