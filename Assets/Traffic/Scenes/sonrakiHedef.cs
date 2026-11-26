using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class sonrakiHedef : MonoBehaviour
{
    public GameObject[] sonrakiler;
    public Color renk;

    public GameObject[] trafikLambalari;
    public float kirmiziIsikZamani = 10;
    public float yesilIsikZamani = 10;
    public bool tersTaraf = false;
    int aktifAn = 0;

    private void Start()
    {
        if (trafikLambalari.Length > 0)
        {
            InvokeRepeating(nameof(TrafikLambaYak), 0, 1f);
        }
    }

    void TrafikLambaYak()
    {
        int i0 = 0, i1 = 1, i2 = 2;
        aktifAn++;
        if (!tersTaraf)
        {
            trafikLambalari[i0].SetActive(aktifAn < kirmiziIsikZamani + 2);
            trafikLambalari[i1].SetActive((aktifAn > kirmiziIsikZamani - 2 && aktifAn < kirmiziIsikZamani + 2) ||
                  (aktifAn >= kirmiziIsikZamani + yesilIsikZamani + 2 && aktifAn <= kirmiziIsikZamani + 2 + yesilIsikZamani + 2));
            trafikLambalari[i2].SetActive(aktifAn >= kirmiziIsikZamani + 2 && aktifAn < kirmiziIsikZamani + 2 + yesilIsikZamani);
        }
        else
        {
            trafikLambalari[i0].SetActive(aktifAn >= yesilIsikZamani + 2 && aktifAn < kirmiziIsikZamani + 2 + yesilIsikZamani + 2);
            trafikLambalari[i1].SetActive((aktifAn >= yesilIsikZamani && aktifAn < yesilIsikZamani + 2) ||
                  (aktifAn > kirmiziIsikZamani + yesilIsikZamani + 2 && aktifAn <= kirmiziIsikZamani + 2 + yesilIsikZamani + 2));
            trafikLambalari[i2].SetActive(aktifAn < yesilIsikZamani);
        }

        if (aktifAn >= kirmiziIsikZamani + 2 + yesilIsikZamani + 2)
        {
            aktifAn = 0;
        }
    }

    public void OnDrawGizmos()
    {
        Gizmos.color = renk;
        foreach (GameObject go in sonrakiler)
            if (go != null)
            {
                Vector3 start = transform.position;
                Vector3 end = go.transform.position;
                Vector3 dir = (end - start).normalized;
                float length = 2;
                Gizmos.DrawLine(start, end);
                Vector3 left = Quaternion.AngleAxis(45, Vector3.up) * (-dir);
                Gizmos.DrawLine(end, end + 0.5f * length * left);
                Vector3 right = Quaternion.AngleAxis(-45, Vector3.up) * (-dir);
                Gizmos.DrawLine(end, end + 0.5f * length * right);
            }
    }
}
