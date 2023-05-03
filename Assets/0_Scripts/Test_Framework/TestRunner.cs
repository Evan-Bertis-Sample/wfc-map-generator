using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TestRunner
{

    public class Test
    {
        private Func<bool> _test;
        private string _testName;

        public Test (string name, Func<bool> test)
        {
            _testName = name;
            _test = test;
        }

        public bool Run(string runnerName)
        {
            bool result =  _test();
            string prefix = (result) ? "[PASSED]" : "[FAILED]";
            string log = ($"{prefix} {runnerName}.{_testName}");
            if (result) Debug.Log(log);
                else Debug.LogError(log);
            return result;
        }
    }

    private string _header;
    private List<Test> _tests;

    public TestRunner(string header)
    {
        _header = header;
        _tests = new List<Test>();
    }

    public void Queue(params (string, Func<bool>)[] tests)
    {
        foreach (var func in tests) Queue(func.Item1, func.Item2);
    }

    public void Queue(string name, Func<bool> test)
    {
        Test t = new Test(name, test);
        _tests.Add(t);
    }

    public void Run()
    {
        Debug.Log("Running: " + _header);
        int passed = 0;
        foreach(Test t in _tests)
        {
            bool result = t.Run(_header);
            if (result) passed++;
        }

        Debug.Log($"{_header} : PASSED {passed} OUT OF {_tests.Count}");
    }
}
