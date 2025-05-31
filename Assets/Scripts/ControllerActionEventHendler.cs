using System.Collections.Generic;
using System;
using System.Diagnostics;
using System.IO;
using System.Globalization;

class ControllerActionEventHendler
{
    public enum ControllerEnum
    {
        left = 0,
        right = 1,
        head = 2
    };
    public enum ButtonEnum
    {
        primary = 0,
        secondary = 1,
        trigger = 2,
        grip = 3
    }
    public UnityEngine.XR.InputDevice targetDeviceRight;
    public UnityEngine.XR.InputDevice targetDeviceLeft;
    public UnityEngine.XR.InputDevice targetDeviceHead;
    public List<ControllerActionFrame> AllFrames = new List<ControllerActionFrame>();
    public long Id = 0;
    public long Time = 0;
    public Stopwatch stopwatch = new Stopwatch();
    public int ListMaxSize = 10000;
    public ControllerActionEventHendler()
    {
        stopwatch.Start();
    }
    ~ControllerActionEventHendler()
    {
        stopwatch.Stop();
    }
    //sprawdzenie czy kliknąłem
    public bool IsClicked(ControllerEnum cE, ButtonEnum bE, bool trigger)//trigger -kliknąłem teraz
    {
        if (AllFrames.Count < 2 && trigger)
            return false;
        ControllerAction cAActual = null;
        ControllerAction cAPrev = null;
        if (cE == ControllerEnum.left)
            cAActual = AllFrames[AllFrames.Count - 1].caL;
        else if (cE == ControllerEnum.right)
            cAActual = AllFrames[AllFrames.Count - 1].caR;
        else if (cE == ControllerEnum.head)
            cAActual = AllFrames[AllFrames.Count - 1].caH;

        if (trigger)
        {
            if (cE == ControllerEnum.left)
                cAPrev = AllFrames[AllFrames.Count - 2].caL;
            else if (cE == ControllerEnum.right)
                cAPrev = AllFrames[AllFrames.Count - 2].caR;
            else if (cE == ControllerEnum.head)
                cAPrev = AllFrames[AllFrames.Count - 2].caH;
        }
        
        if (bE == ButtonEnum.primary)
        {
            if (trigger && cAActual.primaryButton && !cAPrev.primaryButton) return true;
            if (!trigger) return cAActual.primaryButton;
        }
        else if (bE == ButtonEnum.secondary)
        {
            if (trigger && cAActual.secondaryButton && !cAPrev.secondaryButton) return true;
            if (!trigger) return cAActual.secondaryButton;
        }
        else if (bE == ButtonEnum.trigger)
        {
            if (trigger && cAActual.triggerButton && !cAPrev.triggerButton) return true;
            if (!trigger) return cAActual.triggerButton;
        }
        else if (bE == ButtonEnum.grip)
        {
            if (trigger && cAActual.gripButton && !cAPrev.gripButton) return true;
            if (!trigger) return cAActual.gripButton;
        }
        return false;
    }
    public void UpdateFrame()
    {
        ControllerActionFrame caf = new ControllerActionFrame(Id,
            stopwatch.ElapsedMilliseconds,
            targetDeviceRight,
            targetDeviceLeft,
            targetDeviceHead);
        Id++;
        lock (AllFrames)
        {
            AllFrames.Add(caf);
            if (AllFrames.Count > ListMaxSize)
                AllFrames.RemoveAt(0);
            if (Id > 2 * ListMaxSize)
                Id = 0;
        }
    }
    public void SaveAll(string myPath)
    {
        if (AllFrames.Count < 1)
            return;
        string separator = ";";
        CultureInfo culture = CultureInfo.InvariantCulture;
        string resultText = "";
        lock (AllFrames)
        {
            StreamWriter file;
            if(File.Exists(myPath)) file = new StreamWriter(myPath);
            else
            {
                File.Create(myPath);
                file = new StreamWriter(myPath);
            }
            //header
            resultText = "Id" + separator + "Time" + separator;
            string []header = AllFrames[0].caL.GetHeader("L","");
            foreach(var h in header)
            {
                resultText += h + separator;
            }
            header = AllFrames[0].caR.GetHeader("R","");
            foreach(var h in header)
            {
                resultText += h + separator;
            }
            header = AllFrames[0].caR.GetHeader("H","");
            foreach(var h in header)
            {
                resultText += h + separator;
            }
            resultText = resultText.Substring(0, resultText.Length - separator.Length);
            file.WriteLine(resultText);
            //data
            foreach(var cae in AllFrames)
            {
                resultText = cae.Id + separator + cae.Time + separator
                    + cae.caL.ToString(separator, culture) + separator
                    + cae.caR.ToString(separator, culture) + separator
                    + cae.caH.ToString(separator, culture);
                file.WriteLine(resultText);
            }
            file.Close();
        }
    }
    public void ClearAll()
    {
        lock (AllFrames)
            AllFrames.Clear();
    }
    public ControllerActionFrame GetCurrent()
    {
        if (AllFrames.Count > 0)
            return AllFrames[AllFrames.Count - 1];
        return null;
    }
}