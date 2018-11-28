using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawLine : MonoBehaviour {

	const float RADIUS = 25.0f;
	const float MIDDLE_RADIUS = 23.0f;
	const int STATIC_CIRCLE_NUM = 5;
	const float LINE_WIDTH = .2f;
	const float RADIUS_OFFSET = -1.0f;
	const float WIDTH_OFFSET = .1f;
	private int speed = 0;
	private GameObject lineGroup; // for grouping

	LineRenderer lRend;
	HashSet<LineRenderer> crossLineHash;
	HashSet<LineRenderer> measureLineHash;

	public Canvas canvas;
	public GameObject panel;
	public Camera cam;

	// Use this for initialization
	void Start () {

		Input.compass.enabled = true;
		Input.location.Start();

		crossLineHash = createLineRenderHash("CrossLine", 4);
		measureLineHash = createMeasureLineHash("MeasureLine", 360);
		lRend = gameObject.GetComponent<LineRenderer> ();

		drawStaticCircles();
		drawCompassLines();
	}

	// Update is called once per frame
	void Update () {
		drawRadar();
		drawCompassLines();
	}

	private HashSet<LineRenderer> createLineRenderHash(string lineName, int lineNum) {
		HashSet<LineRenderer> hash = new HashSet<LineRenderer>();
		for (int i = 0; i < lineNum; i++) {
			string name = lineName + i;
			LineRenderer line = createLineRenderer(name, LINE_WIDTH);
			hash.Add(line);
		}

		return hash;
	}

	private HashSet<LineRenderer> createMeasureLineHash(string lineName, int lineNum) {
		HashSet<LineRenderer> hash = new HashSet<LineRenderer>();
		for (int i = 0; i < lineNum; i++) {
			float radius = RADIUS / STATIC_CIRCLE_NUM * i;
			float widthOffset = i % 10 == 0 ? WIDTH_OFFSET : 0.0f;

			string name = lineName + i;
			LineRenderer line = createLineRenderer(name, LINE_WIDTH, widthOffset);
			hash.Add(line);
		}

		return hash;
	}

	private void drawStaticCircles() {
		RectTransform panelRect = panel.GetComponent<RectTransform> ();
		float width = panelRect.rect.width;
		float height = panelRect.rect.height;
		RectTransform canvasRect = canvas.GetComponent<RectTransform> ();
		Vector2 pointPos;

		for (int j = 0; j <= STATIC_CIRCLE_NUM; j++) {
			// GameObject container = new GameObject { name = "Circle" + j };
			float radius = RADIUS / STATIC_CIRCLE_NUM * j;
			float lineWidth = .2f;

			int segments = 360;
			string name = "Circle" + j;
			LineRenderer line = createLineRenderer(name, LINE_WIDTH);
			line.useWorldSpace = false;
			line.startWidth = lineWidth;
			line.endWidth = lineWidth;
			line.positionCount = segments + 1;

			// add extra point to make startpoint and endpoint the same to close the circle
			int pointCount = segments + 1;
			Vector3[] points = new Vector3[pointCount];

			for (int i = 0; i < pointCount; i++) {
					float rad = Mathf.Deg2Rad * (i * 360f / segments);
					Vector2 point = new Vector2(Mathf.Cos(rad) * radius,  Mathf.Sin(rad) * radius);
					Vector3 vec = convertWorldToCanvas(point, canvas);
					points[i] = vec;
			}

			line.SetPositions(points);
		}
	}

	private Vector3 convertWorldToCanvas(Vector2 pos, Canvas canvas) {
		RectTransform canvasRect = canvas.GetComponent<RectTransform> ();
		pos.x += canvasRect.transform.position.x;
		pos.y += canvasRect.transform.position.y;
		return new Vector3(pos.x, pos.y, canvasRect.transform.position.z - 10.0f);
	}

	private void drawCompassLines() {
		// NOTE: 十字線を描画
		int j = 0;
		float rotateAngle = 0.0f;

#if UNITY_IOS && !UNITY_EDITOR
	rotateAngle = Input.compass.trueHeading;
#endif

		foreach (LineRenderer line in crossLineHash) {
			float rad = Mathf.Deg2Rad * (j * 90);
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
			float rOffset = j % 10 == 0 ? RADIUS_OFFSET : 0.0f;
			string name = "MeasureLine" + j;
			float rad = Mathf.Deg2Rad * j + rotateAngle;
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
		Vector3 startVec = convertWorldToCanvas(new Vector2(sX, sY), canvas);
		Vector3 endVec = convertWorldToCanvas(new Vector2(eX, eY), canvas);
		line.SetPosition (0, startVec);
		line.SetPosition (1, endVec);
	}

	private void drawRadar() {
		float posX = Mathf.Sin(Time.time) * RADIUS;
		float posY = Mathf.Cos(Time.time) * RADIUS;
		Vector3 startVec = convertWorldToCanvas(new Vector2(0.0f, 0.0f), canvas);
		Vector3 endVec = convertWorldToCanvas(new Vector2(posX, posY), canvas);
		lRend.SetPosition (0, startVec);
		lRend.SetPosition (1, endVec);
	}
}
