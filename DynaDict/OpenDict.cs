﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Environment = System.Environment;
using Android.Support.V7.Widget;

namespace DynaDict
{
    public class OpenDict : Fragment
    {
        private float _viewX;
        private int _CurrentWordID = 0;
        private string _targetDictName = "PassDict";

        DatabaseManager dm = new DatabaseManager();

        Button btOpenDictDelete;
        Button btOpenDictMoveToPass;
        Button btOpenDictMoveToTrash;
        Button btOpenDictRefresh;
        TextView tvWordName ;
        TextView tvPhonics;
        TextView tvChineseDefinition;
        TextView tvEnglishDefinition;
        TextView tvSentences;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
        }

        private void ResetControlText(VocabularyDataModel vdm)
        {
            if (vdm is null)
                return;
            /*
            tvWordName = view.FindViewById<TextView>(Resource.Id.tvWordName);
            tvPhonics = view.FindViewById<TextView>(Resource.Id.tvPhonics);
            tvChineseDefinition = view.FindViewById<TextView>(Resource.Id.tvChineseDefinition);
            tvEnglishDefinition = view.FindViewById<TextView>(Resource.Id.tvEnglishDefinition);
            tvSentences = view.FindViewById<TextView>(Resource.Id.tvSentences);
            */

            tvWordName.Text = vdm.sVocabulary;
            tvPhonics.Text = vdm.sPhonics;
            tvChineseDefinition.Text = string.Join(Environment.NewLine, vdm.sChineseDefinition.ToArray());
            tvEnglishDefinition.Text = string.Join(Environment.NewLine, vdm.sEnglishDefinition.ToArray());
            tvSentences.Text = string.Join(Environment.NewLine, vdm.sSentences.ToArray());
            btOpenDictDelete.Visibility = ViewStates.Visible;
            btOpenDictRefresh.Visibility = ViewStates.Visible;
            return;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment

            var view = inflater.Inflate(Resource.Layout.fragment_opendicts, container, false);

            btOpenDictMoveToTrash = view.FindViewById<Button>(Resource.Id.btOpenDictMoveToTrash);
            btOpenDictMoveToPass = view.FindViewById<Button>(Resource.Id.btOpenDictMoveToPass);
            btOpenDictDelete = view.FindViewById<Button>(Resource.Id.btOpenDictDelete);
            btOpenDictRefresh = view.FindViewById<Button>(Resource.Id.btOpenDictRefresh);

            tvWordName = view.FindViewById<TextView>(Resource.Id.tvWordName);
            tvPhonics = view.FindViewById<TextView>(Resource.Id.tvPhonics);
            tvChineseDefinition = view.FindViewById<TextView>(Resource.Id.tvChineseDefinition);
            tvEnglishDefinition = view.FindViewById<TextView>(Resource.Id.tvEnglishDefinition);
            tvSentences = view.FindViewById<TextView>(Resource.Id.tvSentences);
            TextView tvOpenDictDictName = view.FindViewById<TextView>(Resource.Id.tvOpenDictDictName);
            ScrollView svWord = view.FindViewById<ScrollView>(Resource.Id.svWord);

            //LinearLayout llContainer = view.FindViewById<LinearLayout>(Resource.Id.llContainer);
            Spinner spDictList = view.FindViewById<Spinner>(Resource.Id.spDictList);

            //ArrayAdapter<string> arrayAdapter = new ArrayAdapter<string>( this.Context, Android.Resource.Layout.SimpleSpinnerDropDownItem, dm.GetDictNameList());
            ArrayAdapter<string> arrayAdapter = new ArrayAdapter<string>(this.Context, Android.Resource.Layout.SimpleSpinnerDropDownItem, dm.GetDictNameList());

            spDictList.Adapter = arrayAdapter;
            spDictList.SetSelection(arrayAdapter.GetPosition("PassDict"));

            string sDictName = dm.GetDefaultValueByKey("DictName");
            if (sDictName.Equals(""))
            {
                tvOpenDictDictName.Text = "Please select a Dictionary first in DictList menu.";
                /*
                tvWordName.Text = "";
                tvPhonics.Text = "";
                tvChineseDefinition.Text = "";
                tvEnglishDefinition.Text = "";
                tvSentences.Text = "";
                */
                btOpenDictMoveToTrash.Visibility = Android.Views.ViewStates.Invisible;
                btOpenDictMoveToPass.Visibility = Android.Views.ViewStates.Invisible;
                spDictList.Visibility = Android.Views.ViewStates.Invisible;
                return view;
            }

            string sDictCount = dm.GetTotalWordsNumberByDictName(sDictName);
            tvOpenDictDictName.Text = "Dictionary: " + sDictName + ", " + sDictCount + " words.";
            //no more words in dictionary.
            if ( sDictCount.Equals("0"))
            {
                //btDelete.Enabled = false;
                //btPass.Enabled = false;
                btOpenDictMoveToTrash.Visibility = Android.Views.ViewStates.Invisible;
                btOpenDictMoveToPass.Visibility = Android.Views.ViewStates.Invisible;
                spDictList.Visibility = Android.Views.ViewStates.Invisible;
                return view;
            }

            DictDataModel ddm = dm.GetDictFromDBByName(sDictName);
            if (ddm is null) 
            {
                tvWordName.Text = "There is no dictionary. Please create one first!";
                return view;
            }

            if (ddm.DictWordList.Count > 0)
            {
                _CurrentWordID = 0;


                ResetControlText(ddm.DictWordList[_CurrentWordID]);
                /*
                tvWordName.Text = ddm.DictWordList[_CurrentWordID].sVocabulary;
                tvPhonics.Text = ddm.DictWordList[_CurrentWordID].sPhonics;
                tvChineseDefinition.Text = string.Join(Environment.NewLine, ddm.DictWordList[_CurrentWordID].sChineseDefinition.ToArray());
                tvEnglishDefinition.Text = string.Join(Environment.NewLine, ddm.DictWordList[_CurrentWordID].sEnglishDefinition.ToArray());
                tvSentences.Text = string.Join(Environment.NewLine, ddm.DictWordList[_CurrentWordID].sSentences.ToArray());
                */

            }


            spDictList.ItemSelected += delegate {
                _targetDictName = spDictList.SelectedItem.ToString();
            };

            btOpenDictRefresh.Click += delegate
            {
                dm.RemoveWordFromAllDict(ddm.DictWordList[_CurrentWordID].sVocabulary);
                VocabularyDataModel vdm = new VocabularyDataModel();
                vdm.ExtractDefinitionFromDicCN(ddm.DictWordList[_CurrentWordID].sVocabulary);
                dm.SaveWordToDict(sDictName, vdm);
                ddm.DictWordList[_CurrentWordID] = vdm;

                ResetControlText(ddm.DictWordList[_CurrentWordID]);
            };

            btOpenDictDelete.Click += delegate
            {
                dm.RemoveWordFromAllDict(ddm.DictWordList[_CurrentWordID].sVocabulary);
                ddm.DictWordList.Remove(ddm.DictWordList[_CurrentWordID]);

                //no more words in dictionary.
                if (ddm.DictWordList.Count == 0)
                {
                    tvOpenDictDictName.Text = "Dictionary: " + sDictName + ", 0 words.";
                    btOpenDictMoveToTrash.Enabled = false;
                    btOpenDictMoveToPass.Enabled = false;
                    btOpenDictDelete.Visibility = ViewStates.Invisible;
                    btOpenDictRefresh.Visibility = ViewStates.Invisible;
                    return;
                }

                if (_CurrentWordID == ddm.DictWordList.Count)
                    _CurrentWordID = 0;
                ResetControlText(ddm.DictWordList[_CurrentWordID]);
            };

            btOpenDictMoveToTrash.Click += delegate
            {
                if(!ddm.sDictName.Equals("Trash"))
                    dm.SaveWordToDict("Trash", ddm.DictWordList[_CurrentWordID]);
                dm.RemoveWordFromDict(ddm.sDictName, ddm.DictWordList[_CurrentWordID].sVocabulary);
                ddm.DictWordList.Remove(ddm.DictWordList[_CurrentWordID]);

                //no more words in dictionary.
                if (ddm.DictWordList.Count == 0)
                {
                    tvOpenDictDictName.Text = "Dictionary: " + sDictName + ", 0 words.";
                    btOpenDictMoveToTrash.Enabled = false;
                    btOpenDictMoveToPass.Enabled = false;
                    btOpenDictDelete.Visibility = ViewStates.Invisible;
                    btOpenDictRefresh.Visibility = ViewStates.Invisible;
                    return;
                }

                if (_CurrentWordID == ddm.DictWordList.Count)
                    _CurrentWordID = 0;
                ResetControlText(ddm.DictWordList[_CurrentWordID]);
            };

            btOpenDictMoveToPass.Click += delegate
            {
                if (ddm.sDictName.Equals(_targetDictName)) return; //To move to itself is forbidden.
                dm.SaveWordToDict(_targetDictName, ddm.DictWordList[_CurrentWordID]);
                dm.RemoveWordFromDict(ddm.sDictName, ddm.DictWordList[_CurrentWordID].sVocabulary);
                ddm.DictWordList.Remove(ddm.DictWordList[_CurrentWordID]);

                //no more words in dictionary.
                if (ddm.DictWordList.Count == 0)
                {
                    tvOpenDictDictName.Text = "Dictionary: " + sDictName + ", 0 words.";
                    btOpenDictMoveToTrash.Enabled = false;
                    btOpenDictMoveToPass.Enabled = false;
                    btOpenDictDelete.Visibility = ViewStates.Invisible;
                    btOpenDictRefresh.Visibility = ViewStates.Invisible;
                    return;
                }

                if (_CurrentWordID == ddm.DictWordList.Count)
                    _CurrentWordID = 0;
                ResetControlText(ddm.DictWordList[_CurrentWordID]);
            };

            svWord.Touch += (s, e) =>
            {
                var handled = false;

                switch (e.Event.Action)
                {
                    case MotionEventActions.Down:
                        _viewX = e.Event.GetX();
                        handled = true;
                        break;
                    case MotionEventActions.Up:
                        var left = (int)(_viewX - e.Event.GetX() );
                        if (left > 150)
                        {
                            _CurrentWordID++;
                            if (_CurrentWordID == ddm.DictWordList.Count)
                                _CurrentWordID = 0;

                        }
                        if (left < -150)
                        {
                            _CurrentWordID--;
                            if (_CurrentWordID < 0)
                                _CurrentWordID = ddm.DictWordList.Count - 1;
                        }


                        /*
                        tvWordName.Text = ddm.DictWordList[_CurrentWordID].sVocabulary;
                        tvPhonics.Text = ddm.DictWordList[_CurrentWordID].sPhonics;
                        tvChineseDefinition.Text = string.Join(Environment.NewLine, ddm.DictWordList[_CurrentWordID].sChineseDefinition.ToArray());
                        tvEnglishDefinition.Text = string.Join(Environment.NewLine, ddm.DictWordList[_CurrentWordID].sEnglishDefinition.ToArray());
                        tvSentences.Text = string.Join(Environment.NewLine, ddm.DictWordList[_CurrentWordID].sSentences.ToArray());
                        */
                        if(ddm.DictWordList.Count > 0)
                            ResetControlText(ddm.DictWordList[_CurrentWordID]);

                        handled = true;
                        break;
                }



                /*
                if (e.Event.Action == MotionEventActions.Down)
                {
                    // do stuff
                    handled = true;
                }
                else if (e.Event.Action == MotionEventActions.Up)
                {
                    // do other stuff
                    handled = true;
                }
                */

                e.Handled = handled;
            };




            return view; 

            //return base.OnCreateView(inflater, container, savedInstanceState);
        }
    }
}