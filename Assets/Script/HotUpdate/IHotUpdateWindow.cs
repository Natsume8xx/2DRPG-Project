using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHotUpdateWindow 
{
    public void Show(long allBytes,Action onEnd);
    public void UpdatedBar(long downLoadBytes);
}
