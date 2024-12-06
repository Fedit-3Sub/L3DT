using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VIZToHMF
{

    public class Structure
    {
        public int pIndex;
        public uint ptype;
        public uint index;
        public uint type;
        public string szName;
        public float[] bBox = new float[6];
        public float[] transform = new float[16];
        public uint propIDListCnt;
        public List<uint> propIDList = new List<uint>();
        public uint meshIDListCnt;
        public List<uint> meshIDList = new List<uint>();
        public uint childNodeIDListCnt;
        public List<uint> childNodeIDList = new List<uint>();
        public float[] color = new float[4];

        public Structure()
        {
            pIndex = -1;
            ptype = 0;
            index = 0;
            type = 0;
            propIDListCnt = 0;
            meshIDListCnt = 0;
            childNodeIDListCnt = 0;

            // 배열 초기화 (반복문 사용)
            for (int i = 0; i < bBox.Length; i++) bBox[i] = 0f;
            for (int i = 0; i < transform.Length; i++) transform[i] = 0f;
            for (int i = 0; i < color.Length; i++) color[i] = 0f;
        }
    }

    public enum PropType
    {
        sAVPROPSTRING = 0,
        AVPROPINT = 1,
        AVPROPFLOAT = 2,
        AVPROPDATE = 3,
        AVPROPBOOL = 4,
        AVPROPNOTDEF = 0xffff
    }

    public class PropSubTable
    {
        public string keyStr;    
        public string valStr;    
        public ushort valType;   

        public PropSubTable()
        {
            keyStr = string.Empty;
            valStr = string.Empty;
            valType = 0;
        }
    }

    public class PropTable
    {
        public uint index;
        public uint propCnt;
        public List<string> keyStr = new List<string>();  
        public List<string> valStr = new List<string>();  
        public List<ushort> valType = new List<ushort>(); 

        public PropTable()
        {
            index = 0;
            propCnt = 0;
            Clear();
        }

        public void Clear()
        {
            keyStr.Clear();
            valStr.Clear();
            valType.Clear();
        }
    }

    public class MeshTable
    {
        public uint index;
        public float[] color = new float[4];
        public uint vnSize;
        public uint triSize;
        public List<float> VArray = new List<float>();
        public List<float> NArray = new List<float>();
        public List<ushort> TriArray = new List<ushort>();

        public MeshTable()
        {
            index = 0;
            vnSize = 0;
            triSize = 0;
            Clear();
        }

        public void Clear()
        {
            // 배열 초기화 (반복문 사용)
            for (int i = 0; i < color.Length; i++) color[i] = 0f;
            VArray.Clear();
            NArray.Clear();
            TriArray.Clear();
        }
    }
}
