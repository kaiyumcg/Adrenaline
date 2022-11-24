using PathCreation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityExt;

namespace Adrenaline
{
    public class CurvyObstacle : GameObstacle
    {
        [SerializeField] List<PathCreator> paths;
        [SerializeField] float speed = 5f;
        [SerializeField, Range(0.0f, 1.0f)] float startOffset = 0.1f;
        [SerializeField] Vector3 pathPositionOffset;
        [SerializeField] Transform holder;
        [SerializeField] List<GameObject> targets;
        [SerializeField] List<float> delayPattern;

        #region ForOverrideSupport
        public List<PathCreator> Paths { get { return paths; } set { paths = value; } }
        public float Speed { get { return speed; } set { speed = value; } }
        public float StartOffset { get { return startOffset; } set { startOffset = value; } }
        public Vector3 PathPositionOffset { get { return pathPositionOffset; } set { pathPositionOffset = value; } }
        public List<float> DelayPattern { get { return delayPattern; } set { delayPattern = value; } }
        #endregion
        protected override void OnStartReceiverDeath(IObstacleReceiverActor receiver)
        {
            base.OnStartReceiverDeath(receiver);
            StopAllCoroutines();
            var anims = GetComponentsInChildren<Animator>(true);
            anims.ExForEach((anim) =>
            {
                if (anim != null)
                {
                    anim.cullingMode = AnimatorCullingMode.CullCompletely;
                    anim.SetBool("moving", false);
                    anim.SetFloat("speed", 0.0f);
                }
            });
        }
        protected override void OnStartObstacle()
        {
            base.OnStartObstacle();
            targets.ExForEach((it) => { it.SetActive(false); });
            paths.ExForEach((path) => { StartCoroutine(Follower(path)); });
            IEnumerator Follower(PathCreator path)
            {
                var maxDist = path.path.length;
                int id = 0;
                while (true)
                {
                    var dt = Time.deltaTime;
                    for (int i = 0; i < targets.Count; i++)
                    {
                        var t = targets[i];
                        CloneAndStartMoving(t);
                        float delay = 0.0f;

                        if (id > delayPattern.Count - 1) { id = 0; }
                        delay = delayPattern[id];
                        yield return new WaitForSeconds(delay);
                        id++;
                    }
                    yield return null;
                }
                void CloneAndStartMoving(GameObject item)
                {
                    var orit = item.transform;
                    var obj = Instantiate(item) as GameObject;
                    obj.SetActive(true);
                    var t = obj.transform;
                    t.SetParent(holder, true);
                    t.localPosition = orit.localPosition;
                    t.localEulerAngles = orit.localEulerAngles;
                    t.localScale = orit.localScale;
                    var maxDist = path.path.length;
                    float dist = startOffset * maxDist;

                    var anims = obj.GetComponentsInChildren<Animator>(true);
                    anims.ExForEach((anim) =>
                    {
                        anim.cullingMode = AnimatorCullingMode.CullCompletely;
                        anim.SetBool("moving", true);
                        anim.SetFloat("speed", 1.0f);
                    });

                    StartCoroutine(COR());
                    IEnumerator COR()
                    {
                        while (true)
                        {
                            dist += speed * Time.smoothDeltaTime;
                            if (dist > maxDist)
                            {
                                Destroy(obj);
                                break;
                            }
                            
                            var posFromPathCreator = path.path.GetPointAtDistance(dist);
                            Vector3 tg_pos = posFromPathCreator + pathPositionOffset;
                            Quaternion tg_rot = t.rotation;
                            var p1 = path.path.GetPointAtDistance(dist);
                            var p2 = path.path.GetPointAtDistance(dist + 0.1f);
                            var lookDir = p2 - p1;
                            if (lookDir.magnitude > 0.02f)
                            {
                                var normDir = path.GetNormalAtDistance(dist);
                                normDir = normDir.magnitude < 0.001f ? Vector3.up : normDir;
                                tg_rot = Quaternion.LookRotation(lookDir, normDir);
                            }
                            t.SetPositionAndRotation(tg_pos, tg_rot);
                            yield return null;
                        }
                    }
                }
            }
        }
    }
}