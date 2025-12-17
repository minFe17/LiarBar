using UnityEngine;
using Utils;
using System.Collections.Generic;
using System;

/*
 !매개변수 있는거랑 없는거랑, 혹은 1개인거랑 2개인거랑 섞으시면 안됩니다.
=================================================================================
사용 메뉴얼
1. 이벤트 등록
ex) Subscribe("Key", Func); => 매개변수 1은 키값, 2가 함수명이 됩니다.
*매개변수 있는 이벤트
ex) Subscribe<Type>("Key",Func); => Type엔 매개변수 타입 적어주시고 똑같이 등록해주시면 됩니다.
<Type1, Type2> 도 동일

2. 이벤트 호출
ex) Invoke("Key") => 키값으로만 호출 가능 (매개변수x)
*매개변수 있는 이벤트
ex) Invoke<Type>("Key", 매개변수값) => Type에 타입 적고, 값 넘기면 됩니다. 
ex) Invoke<Type1, Type2>("Key", 매개변수1, 매개변수2) => 두개일경우 이렇게 넘겨주시면 됩니다.

3. 이벤트 등록 해제
ex) UnSubscribe("Key", (Action)Func) => 매개변수 없는 함수는 그냥 이렇게 해제 가능합니다.
ex) UnSubscribe("Key", (Action<Type>)Func) => 매개변수 있는 함수는 이런식으로 형변환해서 넘겨줘야됩니다.

=================================================================================
 리턴값 있는 함수 사용 메뉴얼
1. 이벤트 등록
ex) SubscribeFunc<int>("key", Func); => 리턴받는 형식 템플릿으로 넘겨주시면됩니다.

2. 이벤트 호출
ex) InvokeFunc<int>("Key"); => 함수명만 달라졌으니, 유의해주시기바랍니다.

3. 이벤트 등록 해제
ex) UnSubscribqFunc<int>("Key", Func); => (Action) 생략하셔야됩니다. 
 
//이거 아직 안돼서 사용하지말것.
 
 */

public class EventManager : SimpleSingleton<EventManager>
{
    private Dictionary<string, Delegate> _eventBus = new Dictionary<string, Delegate>();
    private Dictionary<string, Delegate> _funcBus = new Dictionary<string, Delegate>();

    public void SubscribeFunc<T>(string key, Func<T> callBack)
    {
        if (_funcBus.TryGetValue(key, out Delegate existing))
            _funcBus[key] = Delegate.Combine(existing, callBack);
        else
            _funcBus[key] = callBack;
    }

    public void Subscribe(string key, Action callBack)
    {
        AddDelegate(key, callBack);
    }
    public void Subscribe<T>(string key, Action<T> callBack)
    {
        AddDelegate(key, callBack);
    }
    public void Subscribe<T1, T2>(string key, Action<T1, T2> callBack)
    {
        AddDelegate(key, callBack);
    }

    public void UnSubscribeFunc<T>(string key, Func<T> callBack)
    {
        if (_funcBus.TryGetValue(key, out Delegate existing))
        {
            Delegate tempDelegate = Delegate.Remove(existing, callBack);
            if (tempDelegate == null)
                _funcBus.Remove(key);
            else
                _funcBus[key] = tempDelegate;
        }
    }

    public void UnSubscribe(string key, Delegate callBack)
    {
        if (_eventBus.TryGetValue(key, out Delegate existing))
        {
            Delegate tempDelegate = Delegate.Remove(existing, callBack);
            if (tempDelegate == null)
            {
                _eventBus.Remove(key);
            }
            else
            {
                _eventBus[key] = tempDelegate;
            }
        }
    }

    public T InvokeFunc<T>(string key)
    {
        if (_funcBus.TryGetValue(key, out Delegate existing) && existing is Func<T> callBack)
        {
            return callBack.Invoke();
        }
        throw new Exception("Key not found or wrong type");
    }

    public void Invoke(string key)
    {
        if (_eventBus.TryGetValue(key, out Delegate existing))
        {
            if(existing is Action callBack)
            {
                callBack.Invoke();
            }
        }
        else
        {
            Debug.Log(key + "오류발생");
        }
    }
    public void Invoke<T>(string key, T param)
    {
        if (_eventBus.TryGetValue(key, out Delegate existing))
        {
            if (existing is Action<T> callBack)
            {
                callBack.Invoke(param);
            }
        }
        else
        {
            Debug.Log(key + "오류발생");
        }
    }
    public void Invoke<T1,T2>(string key, T1 param1, T2 param2)
    {
        Delegate[] invocationList = _eventBus[key].GetInvocationList();
        int count = invocationList.Length;
        Debug.Log(count.ToString());
        if (_eventBus.TryGetValue(key, out Delegate existing))
        {
            if (existing is Action<T1,T2> callBack)
            {
                callBack.Invoke(param1, param2);
            }
        }
        else
        {
            Debug.Log(key + "오류발생");
        }
    }

    private void AddDelegate(string key, Delegate callBack)
    {
        if (_eventBus.TryGetValue(key, out Delegate existing))
        {
            if (existing.GetType() == callBack.GetType())
            {
                _eventBus[key] = Delegate.Combine(existing, callBack);
                return;
            }
        }
        else
        {
            _eventBus[key] = callBack;
        }
    }
}