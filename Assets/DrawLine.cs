using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawLine : MonoBehaviour {

	const float RADIUS = 2.0f;
	const float MIDDLE_RADIUS = 1.8f;
	const int STATIC_CIRCLE_NUM = 5;

	private int speed = 0;
	LineRenderer lRend;

	HashSet<LineRenderer> crossLineHash;

	HashSet<LineRenderer> measureLineHash;

	// Use this for initialization
	void Start () {
		lRend = gameObject.GetComponent<LineRenderer> ();
		lRend.SetVertexCount(2);
		Vector3 startVec = new Vector3 (0.0f, 0.0f, 0.0f);
		Vector3 endVec = new Vector3 (0.0f, 5.0f, 5.0f);
		lRend.SetPosition (0, startVec);
		lRend.SetPosition (1, endVec);

		Input.compass.enabled = true;
		Input.location.Start();

		crossLineHash = createLineRenderHash("CrossLine", 4);
		measureLineHash = createMeasureLineHash("MeasureLine", 360);
		drawStaticCircles();
		drawCompassLines();

	}

	private HashSet<LineRenderer> createLineRenderHash(string lineName, int lineNum) {
		HashSet<LineRenderer> hash = new HashSet<LineRenderer>();
		for (int i = 0; i < lineNum; i++) {
			string name = lineName + i;
			LineRenderer line = createLineRenderer(name, .02f);
			hash.Add(line);
		}

		return hash;
	}

	private HashSet<LineRenderer> createMeasureLineHash(string lineName, int lineNum) {
		HashSet<LineRenderer> hash = new HashSet<LineRenderer>();
		for (int i = 0; i < lineNum; i++) {
			float radius = RADIUS / STATIC_CIRCLE_NUM * i;
			float rOffset = i % 10 == 0 ? -0.1f : 0.0f;
			float widthOffset = i % 10 == 0 ? .01f : 0.0f;

			string name = lineName + i;
			LineRenderer line = createLineRenderer(name, .01f, widthOffset);
			hash.Add(line);
		}

		return hash;
	}

	private void drawStaticCircles() {
		for (int j = 0; j <= STATIC_CIRCLE_NUM; j++) {
			// GameObject container = new GameObject { name = "Circle" + j };
			float radius = RADIUS / STATIC_CIRCLE_NUM * j;
			float lineWidth = .02f;

			int segments = 360;
			// LineRenderer line = container.AddComponent<LineRenderer>();
			string name = "Circle" + j;
			LineRenderer line = createLineRenderer(name, .02f);
			line.useWorldSpace = false;
			line.startWidth = lineWidth;
			line.endWidth = lineWidth;
			line.positionCount = segments + 1;

			int pointCount = segments + 1; // add extra point to make startpoint and endpoint the same to close the circle
			Vector3[] points = new Vector3[pointCount];

			for (int i = 0; i < pointCount; i++) {
					float rad = Mathf.Deg2Rad * (i * 360f / segments);
					points[i] = new Vector3(Mathf.Cos(rad) * radius, Mathf.Sin(rad) * radius, 0.0f);
			}

			line.SetPositions(points);
		}
	}

	private void drawCompassLines() {
		// NOTE: 十字線を描画
		int j = 0;
		float rotateAngle = 0.0f;

#if UNITY_IOS && !UNITY_EDITOR
	rotateAngle = Input.compass.trueHeading;
	Debug.Log("rotate angle is " + rotateAngle);
#endif

		foreach (LineRenderer line in crossLineHash) {
			float rad = Mathf.Deg2Rad * (j * 90 + rotateAngle);
			float posX = Mathf.Cos(rad) * RADIUS;
			float posY = Mathf.Sin(rad) * RADIUS;
			drawLine(line, 0.0f, 0.0f, posX, posY);
			j++;
		}

		// NOTE: 外周のメモリを描画
		int step = 1;
		j = 0;
		foreach (LineRenderer line in measureLineHash) {
			float radius = RADIUS / STATIC_CIRCLE_NUM * j;
			float rOffset = j % 10 == 0 ? -0.1f : 0.0f;
			float widthOffset = j % 10 == 0 ? .01f : 0.0f;

			string name = "MeasureLine" + j;
			float rad = Mathf.Deg2Rad * j;
			float posSX = Mathf.Cos(rad) * (MIDDLE_RADIUS + rOffset);
			float posSY = Mathf.Sin(rad) * (MIDDLE_RADIUS + rOffset);;
			float posX = Mathf.Cos(rad) * RADIUS;
			float posY = Mathf.Sin(rad) * RADIUS;
			drawLine(line, posSX, posSY, posX, posY);

			j += step;
		}
	}

	private LineRenderer createLineRenderer(string name, float width, float offset = 0.0f) {
		GameObject container = new GameObject { name = name };
		LineRenderer line = container.AddComponent<LineRenderer>();
		Color c1 = new Color(0.0f, 1.0f, 0.0f, 1);

		line.material = new Material(Shader.Find("Mobile/Particles/Additive"));
		line.SetColors(c1, c1);
		line.useWorldSpace = false;
		line.startWidth = width + offset;
		line.endWidth = width + offset;
		return line;
	}

	private void drawLine(LineRenderer line, float sX, float sY, float eX, float eY) {
		Vector3 startVec = new Vector3 (sX, sY, 0.0f);
		Vector3 endVec = new Vector3 (eX, eY, 0.0f);
		line.SetPosition (0, startVec);
		line.SetPosition (1, endVec);
	}

	// Update is called once per frame
	void Update () {
		float posX = Mathf.Sin(Time.time) * RADIUS;
		float posY = Mathf.Cos(Time.time) * RADIUS;
		Vector3 startVec = new Vector3 (0.0f, 0.0f, 0.0f);
		Vector3 endVec = new Vector3 (posX, posY, 0.0f);
		lRend.SetPosition (0, startVec);
		lRend.SetPosition (1, endVec);

		drawCompassLines();
		speed++;
	}
}
