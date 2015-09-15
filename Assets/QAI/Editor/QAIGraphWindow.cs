using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using UnityEditor;
using UnityEngine;

namespace QAI.Editor {
    public class QAIGraphWindow : EditorWindow {

        private bool _foldoutActions;
        private bool _foldoutIterations;

        private LineChart _actionChart;
        private readonly List<Vector<float>> _actionData = new List<Vector<float>>();
        private const int MaxActionData = 100;

        private LineChart _iterationChart;
        private readonly List<float>[] _iterationData = { new List<float>() };
        private readonly List<float> _currentIterationData = new List<float>(); 
        private int _currentIteration = 0;



        [MenuItem("QAI/Data")]
        private static void OpenWindow() {
            var window = (QAIGraphWindow) GetWindow(typeof (QAIGraphWindow));
            window.Show();
        }

        private void Init() {
            QAIManager.NetworkValuesUpdated += StoreValues;
        }

        private void OnGUI() {
            Init ();

            if(_foldoutActions = EditorGUILayout.Foldout(_foldoutActions, "Action taken")) {
                if(_actionChart == null) {
                    _actionChart = new LineChart(this, 200f) {
                        formatString = "{0:F}", 
                        gridLines = 5, 
                        axisRounding = 1f
                    };
                }

                List<List<float>> l = null;
                foreach(var v in _actionData) {
                    if(l == null) {
                        l = v.Select(e => new List<float>()).ToList();
                    }
                    for(var i = 0; i < v.Count; i++) {
                        l[i].Add(v[i]);
                    }
                }
                if(l == null) return;

                _actionChart.data = l.ToArray();
                _actionChart.DrawChart();
            }
            if (_foldoutIterations = EditorGUILayout.Foldout(_foldoutIterations, "Avg. predicted reward over iterations")) {
                if (_iterationChart == null) {
                    _iterationChart = new LineChart(this, 200f) {
                        formatString = "{0:F}",
                        gridLines = 5,
                        axisRounding = 1f,
                        pipRadius = 1.5f,                        
                        colors = new List<Color> { Color.blue }
                    };
                }
                _iterationChart.data = _iterationData;
                _iterationChart.DrawChart();
            }
        }

        private void StoreValues(Vector<float> data, bool trainingData) {
            if(trainingData) return;
            
            if(_actionData.Count > MaxActionData) _actionData.Remove(_actionData.First());
            if(_actionData.Count != 0 && _actionData[_actionData.Count-1].Equals(data)) return;
            _actionData.Add(data.Clone());

            if (_currentIteration != QAIManager.Iteration) {
                _currentIteration = QAIManager.Iteration;
                if(_currentIterationData.Count > 0)
                    _iterationData[0].Add( _currentIterationData.Average() );
                _currentIterationData.Clear();
            }
            _currentIterationData.Add(data.Maximum());

            Repaint();
        }
    }
}
