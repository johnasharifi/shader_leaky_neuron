using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScriptComputeShaderManager : MonoBehaviour {
	public const int width = 512;
	public const int height = 512;

	private const float growth_interval = 0.06f;
	private const int kernel_dispatch_size = 16;

	private Renderer rend;
	private Material m;
	private RenderTexture tex;

	public ComputeShader cs;
	private int kernelHandle_iterate;

	private Dictionary<string, ComputeBuffer> buffer_manifest = new Dictionary<string, ComputeBuffer> ();

	private float t_next = 0f;

	// Use this for initialization
	void Start () {
		rend = transform.GetComponent<Renderer> ();

		BootKernel ();
	}
	
	// Update is called once per frame
	void Update () {
		if (Time.timeSinceLevelLoad > t_next) {
			t_next = Time.timeSinceLevelLoad + growth_interval;
			IterateKernel ();
		}
	}

	void BootKernel() {
		kernelHandle_iterate = cs.FindKernel ("CSIterate");

		tex = new RenderTexture(width, height,24);
		tex.enableRandomWrite = true;
		tex.filterMode = FilterMode.Point;
		tex.Create();

		cs.SetTexture (kernelHandle_iterate, "Result", tex);

		cs.SetInt ("height", height);
		cs.SetInt ("width", width);
		cs.SetFloat ("growth_interval", growth_interval);

		NameDataPair b_growth = new NameDataPair () {name = "buffer_growth", vals = GetRandomFloatArr(width * height, percent: 1f, max: 0.08f)};
		NameDataPair b_act = new NameDataPair () {name = "buffer_activation", vals = new float[width * height] };
		NameDataPair b_leak = new NameDataPair () {name = "buffer_leak", vals = new float[width * height]};
		foreach (NameDataPair ndp in new NameDataPair[] {b_growth, b_act, b_leak}) {
			WriteBuffer (ndp.vals, ndp.name);
		}

		rend.material.mainTexture = tex;

	}

	void IterateKernel() {
		cs.Dispatch (kernelHandle_iterate, width / kernel_dispatch_size, height / kernel_dispatch_size, 24);
	}

	private void OnDestroy() {
		foreach (KeyValuePair<string, ComputeBuffer> kvp in buffer_manifest)
			kvp.Value.Dispose ();
		buffer_manifest = null;
	}

	private class NameDataPair {
		public string name {get; set;}
		public float[] vals {get; set;}
		};

	private void WriteBuffer(float[] data, string buffer_name) {
		if (!buffer_manifest.ContainsKey (buffer_name)) {
			ComputeBuffer cb = new ComputeBuffer (data.Length, sizeof(float));
			cs.SetBuffer (kernelHandle_iterate, buffer_name, cb);
			buffer_manifest [buffer_name] = cb;
		}

		buffer_manifest[buffer_name].SetData (data);
		cs.SetBuffer(kernelHandle_iterate, buffer_name, buffer_manifest[buffer_name]);
	}

	private float[] ReadBuffer(int dims, string buffer_name) {
		if (!buffer_manifest.ContainsKey (buffer_name)) {
			ComputeBuffer cb = new ComputeBuffer (dims, sizeof(float));
			buffer_manifest [buffer_name] = cb;
		}

		float[] data = new float[dims];
		buffer_manifest[buffer_name].GetData (data);

		return(data);
	}

	private float[] GetRandomFloatArr(int dim, float percent, float max) {
		float[] data = new float[dim];
		for (int i = 0; i < percent * dim; i++)
			data [Random.Range (0, dim)] = Random.Range (0f, max);
		return(data);
	}

}
