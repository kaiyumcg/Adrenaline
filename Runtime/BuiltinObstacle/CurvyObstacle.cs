using PathCreation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityExt;

namespace Adrenaline
{
    [System.Serializable]
    public class DelayPattern
    {
        [SerializeField] internal List<float> delayPattern;
    }


    public class CurvyObstacle : GameObstacle
    {
        [SerializeField] List<PathCreator> paths;
        [SerializeField] float speed = 5f;
        [SerializeField] Vector3 pathPositionOffset;
        [SerializeField] Transform holder;
        [SerializeField] List<GameObject> targets;
        [SerializeField] List<DelayPattern> delayPatterns;
        [SerializeField] bool isLooped = false;
        [SerializeField] bool randomizedDelayPattern = false;

        protected override void OnStartReceiverDeath(IObstacleReceiverActor reciever)
        {
            base.OnStartReceiverDeath(reciever);
            StopAllCoroutines();
            var anims = GetComponentsInChildren<Animator>(true);
            anims.ExForEachSafe((anim) =>
            {
                anim.SetBool("moving", false);
                anim.SetFloat("speed", 0.0f);
            });
        }
        protected override void OnStartObstacle()
        {
            base.OnStartObstacle();
            targets.ExForEachSafe((it) => { it.SetActive(false); });
            paths.ExForEachSafe((path, index) => { StartCoroutine(Follower(path, index)); });
            IEnumerator Follower(PathCreator path, int index)
            {
                var maxDist = path.path.length;
                int id = 0;
                this.targets.ExShuffle();
                var patterns = delayPatterns[index].delayPattern;
                if (randomizedDelayPattern)
                {
                    patterns.ExShuffle();
                }

                var d = maxDist;
                var startPatternIndex = 0;
                var startTargetIndex = 0;
                while (d > 0.0f)
                {
                    d -= speed * patterns[startPatternIndex];
                    CloneAndStartMoving(targets[startTargetIndex], manualStartPosition: true, d);

                    startPatternIndex++;
                    if (startPatternIndex > patterns.Count - 1) { startPatternIndex = 0; }
                    startTargetIndex++;
                    if (startTargetIndex > targets.Count - 1) { startTargetIndex = 0; }
                }

                if (!isLooped)
                {
                    while (true)
                    {
                        var dt = Time.deltaTime;
                        for (int i = 0; i < targets.Count; i++)
                        {
                            float delay = 0.0f;
                            if (id > patterns.Count - 1) { id = 0; }
                            delay = patterns[id];
                            yield return new WaitForSeconds(delay);
                            id++;

                            var t = targets[i];
                            CloneAndStartMoving(t);
                        }
                        yield return null;
                    }
                }
                void CloneAndStartMoving(GameObject item, bool manualStartPosition = false, float manualDistance = 0.0f)
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
                    float dist = manualStartPosition ? manualDistance : 0.0f;

                    var anims = obj.GetComponentsInChildren<Animator>(true);
                    anims.ExForEachSafe((anim) =>
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
                            dist += speed * Time.deltaTime;
                            if (dist > maxDist)
                            {
                                if (isLooped)
                                {
                                    var delta = dist - maxDist;
                                    dist = delta;
                                }
                                else
                                {
                                    Destroy(obj);
                                    break;
                                }
                            }

                            var posFromPathCreator = path.path.GetPointAtDistance(dist);
                            Vector3 tg_pos = posFromPathCreator + pathPositionOffset;
                            Quaternion tg_rot = t.rotation;
                            var p1 = path.path.GetPointAtDistance(dist);
                            var p2 = path.path.GetPointAtDistance(dist + 0.1f);

                            var secondDist = dist + 0.1f;
                            if (secondDist > maxDist)
                            {
                                p1 = path.path.GetPointAtDistance(maxDist - 0.1f);
                                p2 = path.path.GetPointAtDistance(maxDist);
                            }

                            var lookDir = p2 - p1;
                            if (lookDir.magnitude > 0.02f)
                            {
                                var normDir = path.GetNormalAtDistance(dist);
                                normDir = normDir.magnitude < 0.001f ? Vector3.up : normDir;
                                tg_rot = Quaternion.LookRotation(lookDir, normDir);
                            }
                            t.position = tg_pos;
                            t.rotation = Quaternion.Slerp(t.rotation, tg_rot, 12f * Time.deltaTime);
                            //t.SetPositionAndRotation(tg_pos, tg_rot);
                            yield return null;
                        }
                    }
                }
            }
        }
    }
}