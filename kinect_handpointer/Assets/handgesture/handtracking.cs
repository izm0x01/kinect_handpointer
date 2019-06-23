using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Kinect = Windows.Kinect;


public class handtracking : MonoBehaviour {

    public GameObject BodySourceManager;

    [SerializeField]private Camera convertcamera;

    private Kinect.CoordinateMapper _CoordinateMapper = null;

    private Dictionary<ulong, GameObject> _Bodies = new Dictionary<ulong, GameObject>();
    private BodySourceManager _BodyManager;

    private int _KinectWidth = 1920;
    private int _KinectHeight = 1080;

    [Header("文字により手の状態を知る。グー＝Closed、チョキ＝Lasso、パ＝Open")]
    [TextArea(2, 5)] public string righthandstate;
    [TextArea(2, 5)] public string lefthandstate;

    Vector3 _righthand_pos;
    public Vector3 righthand_pos
    {
        get { return _lefthand_pos; }
    }
    Vector3 _lefthand_pos;
    public Vector3 lefthand_pos
    {
        get { return _righthand_pos; }
    }

    void Update()
    {
        if (BodySourceManager == null)
        {
            return;
        }

        //BodySourceManagerがアタッチされているか
        _BodyManager = BodySourceManager.GetComponent<BodySourceManager>();
        if (_BodyManager == null)
        {
            return;
        }

        //CoordinateMapperは座標変換に使います。
        if (_CoordinateMapper == null)
        {
            _CoordinateMapper = _BodyManager.Sensor.CoordinateMapper;
        }

        //体のトラッキングはできているか
        Kinect.Body[] data = _BodyManager.GetData();
        if (data == null)
        {
            return;
        }

        List<ulong> trackedIds = new List<ulong>();
        foreach (var body in data)
        {
            if (body == null)
            {
                continue;
            }

            if (body.IsTracked)
            {
                trackedIds.Add(body.TrackingId);
            }
        }

        List<ulong> knownIds = new List<ulong>(_Bodies.Keys);

        // First delete untracked bodies
        foreach (ulong trackingId in knownIds)
        {
            if (!trackedIds.Contains(trackingId))
            {
                Destroy(_Bodies[trackingId]);
                _Bodies.Remove(trackingId);
            }
        }

        foreach (var body in data)
        {
            if (body == null)
            {
                continue;
            }
            
            if (body.IsTracked)
            {
                if (!_Bodies.ContainsKey(body.TrackingId))
                {
                    _Bodies[body.TrackingId] = CreateHandObject(body.TrackingId);
                }

                RefreshHandObject(body, _Bodies[body.TrackingId]);
            }
        }
    }

    private GameObject CreateHandObject(ulong id)
    {
        GameObject hands = new GameObject("hands:" + id);

        GameObject Righthand = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Righthand.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        Righthand.name = Kinect.JointType.HandRight.ToString();
        Righthand.transform.parent = hands.transform;

        GameObject Lefthand = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Lefthand.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        Lefthand.name = Kinect.JointType.HandLeft.ToString();
        Lefthand.transform.parent = hands.transform;

        return hands;
    }

    private void RefreshHandObject(Kinect.Body body, GameObject bodyObject)
    {
            Transform Lefthand = 
            bodyObject.transform.Find(Kinect.JointType.HandLeft.ToString());

            Transform Righthand =
            bodyObject.transform.Find(Kinect.JointType.HandRight.ToString());

            if (Lefthand == null || Righthand == null) return;

            Lefthand.localPosition = 
            GetVector3FromJoint(body.Joints[Kinect.JointType.HandLeft]);

            Righthand.localPosition = 
            GetVector3FromJoint(body.Joints[Kinect.JointType.HandRight]);

            _righthand_pos = Righthand.localPosition;
            _lefthand_pos = Lefthand.localPosition;

        lefthandstate = body.HandLeftState.ToString();
        righthandstate = body.HandRightState.ToString();
    }

    private Vector3 GetVector3FromJoint(Kinect.Joint joint)
    {
        var valid = joint.TrackingState != Kinect.TrackingState.NotTracked;
        if (convertcamera != null || valid)
        {
            // KinectのCamera座標系(3次元)をColor座標系(2次元)に変換する
            var point = _CoordinateMapper.MapCameraPointToColorSpace(joint.Position);
            var point2 = new Vector3(point.X, point.Y, 0);
            if ((0 <= point2.x) && (point2.x < _KinectWidth) &&
                 (0 <= point2.y) && (point2.y < _KinectHeight))
            {

                // スクリーンサイズで調整(Kinect->Unity)
                point2.x = point2.x * Screen.width / _KinectWidth;
                point2.y = point2.y * Screen.height / _KinectHeight;

                // Unityのワールド座標系(3次元)に変換
                var colorPoint3 = convertcamera.ScreenToWorldPoint(point2);

                // 座標の調整
                // Y座標は逆、Z座標は-1にする(Xもミラー状態によって逆にする必要あり)
                colorPoint3.y *= -1;
                colorPoint3.z = -1;

                return colorPoint3;
            }
        }
        return new Vector3(joint.Position.X * 10, joint.Position.Y * 10, -1);
    }
}
