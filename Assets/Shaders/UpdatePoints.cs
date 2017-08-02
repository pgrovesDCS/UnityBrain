using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdatePoints : MonoBehaviour {
    public Shader shader1;
    public Renderer rr;
    public Rigidbody rb;
    public Transform playerTransform;
    public GameObject cube;
    // Use this for initialization
    void Start () {
        var materialProperty = new MaterialPropertyBlock();
        float[] floatArray = new float[] { 1.0f, 1.0f, 0.0f, 1.0f };
        //materialProperty.SetFloatArray("cc", floatArray);
        //gameObject.GetComponent<Renderer>().SetPropertyBlock(materialProperty);
        rr = gameObject.GetComponentInChildren<Renderer>();//.material.SetFloatArray("cc",floatArray);
        rr.material.SetFloatArray("cc", floatArray);
        // playerTransform = GetComponent<Transform>();
        // renderer = GetComponent<Renderer>();
        // this.renderer.material.SetColor("color", Color.red);
        // renderer.material.SetFloatArray("color", new float[] { 1.0f, 0.0f, 0.0f, 0.0f });
        // shader1 = Shader.Find("Unlit/NewUnlitShader");// renderer
    }

    // Update is called once per frame
    void Update () {
        transform.Rotate(new Vector3(0,1,0), 1.0f);

        Vector4[] points = new Vector4[]
        {
            new Vector4(0,2.0f,0,1),
            new Vector4(2,0,0,1),
            new Vector4(0,0,2,1),
            new Vector4(-20,0,0,1),
            new Vector4(-30,0,0,1),
            new Vector4(-40,0,0,1)
        };

        Vector4[] colors = new Vector4[]{
            new Vector4(0,0,1f,.5f),
            new Vector4(1,0,0,.5f),
            new Vector4(0,1,0,.5f),
            new Vector4(1,0,1,.5f),
            new Vector4(1,0,.5f,.5f),
            new Vector4(1,1,1,.5f)
        };

        rr.material.SetInt("electrodeCount", 6);
        rr.material.SetVectorArray("dataLocation", points);
        rr.material.SetVectorArray("dataColor", colors);
        //renderer.material.shader
    }
}
