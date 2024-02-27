using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Battler))]
public class KeiAndroidManagement : MonoBehaviour
{
    [Header("Debug")]
    Battler kei;
    Battler[] spawnedAndroid = new Battler[2];
    Battle battleManager;
    List<EnemyDefine> possibleSpawn; 

    // Start is called before the first frame update
    void Start()
    {
        battleManager = FindObjectOfType<Battle>();
        if (battleManager == null) return; // �o�g����ʂł͂Ȃ�

        kei.onDeathEvent.AddListener(OnDead);

        // ������
        for (int i = 0; i < spawnedAndroid.Length; i++)
        {
            spawnedAndroid[i] = null;
        }

        // ���������ł��郂�m�����[�h
        possibleSpawn = new List<EnemyDefine>();
        possibleSpawn.Add(Resources.Load<EnemyDefine>("EnemyList/" + "Drone 4"));
        possibleSpawn.Add(Resources.Load<EnemyDefine>("EnemyList/" + "Android 1"));
        possibleSpawn.Add(Resources.Load<EnemyDefine>("EnemyList/" + "GoldDrone 2"));
        possibleSpawn.Add(Resources.Load<EnemyDefine>("EnemyList/" + "GoldAndroid 3"));
        possibleSpawn.Add(Resources.Load<EnemyDefine>("EnemyList/" + "DarkAndroid 2"));
        possibleSpawn.Add(Resources.Load<EnemyDefine>("EnemyList/" + "DarkAndroid 3"));
    }

    public void SpawnNewAndroid()
    {
        // �ǂ̃L��������������̂����߂�
        EnemyDefine targetSummon = possibleSpawn[UnityEngine.Random.Range(0, possibleSpawn.Count)];

        {
            // ����
            GameObject obj = Instantiate<GameObject>(targetSummon.battler, kei.transform.parent);

            // �ʒu�����߂�
            obj.transform.localPosition = new Vector3(positionX, positionY, 0.0f);
            obj.transform.SetSiblingIndex(siblingIndex);

            // �G��������
            Battler component = obj.GetComponent<Battler>();
            component.InitializeEnemyData(targetSummon);

            // �^�[�����ʂ̍Ō���ɒu��
            battleManager.AddEnemy(component);

            // �퓬���O
            battleManager.AddBattleLog("abc");
        }
    }

    // ������񂪃��^�C�A
    public void OnDead()
    {
        // �����ƈꏏ�Ƀ^�|����
        for (int i = 0; i < spawnedAndroid.Length; i ++)
        {
            if (spawnedAndroid[i] != null)
            {
                spawnedAndroid[i].KillBattler();
            }
        }
    }
}
