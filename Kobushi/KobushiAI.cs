using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class KobushiAI : MonoBehaviour
{
    // Start is called before the first frame update
    

    const float TIME_OUT=3f;        //攻撃間隔
    const float TARAGET_SEARCH=20f; //索敵範囲
    const float TARGET_DISTANCE=1f; //対象との適正距離
    const float SPEED=3.5f;         //初期速度
    const float ANGULAR_SPEED=3f;    //回転速度
    const float BACK_SPEED=-0.02f;  //後退速度
    const int MAX_HP=20;            //最大HP
    const int TYPES_OF_ATTACK=2;    //現在ある攻撃の種類
    class Attack{
        private Animator animator;

        private float start;            //当たり判定開始時
        private float end;              //当たり判定終了時
        private int damage;             //攻撃ダメージ
        private int inpactLevel;         //ヒット時ののけぞり威力
        private float speed;            //攻撃時速度
        private float angularSpeed;     //攻撃時回転速度
        private Collider[] colliders;    //攻撃部位
        Attack(float start,float end,int damage,int inpactLevel,float speed,float angularSpeed,Collider[] colliders){
            this.start=start;
            this.end=end;
            this.damage=damage;
            this.inpactLevel=inpactLevel;
            this.speed=speed;
            this.angularSpeed=angularSpeed;
            this.colliders=colliders;
        }
        void play(){
            float movingTime = animator.GetFloat("MovingTime");//MovingTimeが取れない
            if(start<=movingTime&&movingTime<=end){
                foreach(Collider collider in colliders) collider.enabled = true;
            }else{
                foreach(Collider collider in colliders) collider.enabled = false;
            }
        }
        Attack jab = new Attack(0.1f,0.3f,
                                5,1,
                                SPEED,ANGULAR_SPEED,
                                new Collider[] {GameObject.Find("RightArm").GetComponent<SphereCollider>()}
                                );//ジャブ実装
        
    };

    public GameObject TargetObject; ///目標位置
    NavMeshAgent m_navMeshAgent; /// NavMeshAgent
    Animator animator;

    private float timeElapsed;
    void Start()
    {
        // NavMeshAgent コンポーネントを取得
        m_navMeshAgent=GetComponent<NavMeshAgent>();
        m_navMeshAgent.speed       =SPEED;
        m_navMeshAgent.angularSpeed=ANGULAR_SPEED;

        animator = GetComponentInChildren<Animator>();
        animator.SetInteger("HP",MAX_HP);
        TargetObject= GameObject.Find("unitychan_dynamic");
    }

    // Update is called once per frame
    void Update()
    {
        if(animator.GetInteger("HP")>0&&animator.GetBool("Hit")==false){
            float dis = Vector3.Distance(this.transform.position,TargetObject.transform.position);
            if(dis<TARAGET_SEARCH){//索敵範囲に入ったら追跡
                // NavMesh が準備できているなら
                if(m_navMeshAgent.pathStatus != NavMeshPathStatus.PathInvalid){
                    // NavMeshAgent に目的地をセット
                    m_navMeshAgent.SetDestination(TargetObject.transform.position);
                }
                // 一定時間ごとに弾を射出する
                timeElapsed += Time.deltaTime;
                if(timeElapsed>=TIME_OUT){
                    int attack=Random.Range(1, TYPES_OF_ATTACK+1);
                    animator.SetInteger("Attack",attack);
                    switch(attack){
                        case 1:
                            timeElapsed=1.0f;
                            break;
                        case 2:
                            timeElapsed=0.0f;
                            break;
                    }
                    Invoke("reset",0.1f);
                }


                if(m_navMeshAgent.remainingDistance<TARGET_DISTANCE){//近づきすぎた場合に後ろへ下がる
                    this.transform.position += this.transform.forward * BACK_SPEED;
                }

                //　プレイヤーの方向を取得
                var playerDirection = new Vector3(TargetObject.transform.position.x, transform.position.y, TargetObject.transform.position.z) - transform.position;
                //　敵の向きをプレイヤーの方向に少しづつ変える
                var dir = Vector3.RotateTowards(transform.forward, playerDirection, m_navMeshAgent.angularSpeed * Time.deltaTime, 0f);
                //　算出した方向の角度を敵の角度に設定
                transform.rotation = Quaternion.LookRotation(dir);
            }else{
                m_navMeshAgent.SetDestination(this.transform.position);
            }

        }
        if(animator.GetBool("Death"))Destroy(this.gameObject);
    }

    void reset(){
        animator.SetInteger("Attack",0);
        m_navMeshAgent.speed       =SPEED;
        m_navMeshAgent.angularSpeed=ANGULAR_SPEED;

    }
}
