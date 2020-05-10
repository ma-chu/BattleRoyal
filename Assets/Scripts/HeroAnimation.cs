using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#pragma warning disable 0649    // убирает предупреждения компилятора о [SerializeField] private переменных, инициализируемых в редакторе   
//  ДВИЖЕНИЕ И АНИМАЦИЯ ГЕРОЯ
public class HeroAnimation : MonoBehaviour
{
    // СОСТОЯНИЯ
    [SerializeField]
    private bool m_change;
    [SerializeField]
    private bool m_toPosition;                           // Выход на ристалище - в начале боя и после смены оружия
    [SerializeField]
    private bool m_rotateToCenter;                       // Разворот после смены оружия

    private float zeroZposition;                         // координата Z позиции на ристалище 
    private float zeroYrotation;                         // вращение позиции на ристалище
    private float stockXposition;                        // координата Z позиции склада оружия

    [SerializeField]
    private Animator m_Anim;                             // ссылка на компонент-аниматор этого героя
    [SerializeField]
    private RuntimeAnimatorController m_ACSwordShield;   // ссылки на контроллеры анимации
    [SerializeField]
    private AnimatorOverrideController m_AC2HandedSword;
    [SerializeField]
    private AnimatorOverrideController m_ACSwordSword;

    [SerializeField]                            // может, как-то получить через GetComponent?
    private HeroManager heroManager;

    private void _Awake()
    {
        // получаем ссылку на компонент-аниматор этого героя (на случай, если игру расширю и героя придется порождать в процессе игры)
        //m_Anim = this.GetComponentInChildren<Animator>();  //ПОЧЕМУ не РАБОТАЕТ - было ж норм???
    }

    private void OnEnable()    
    {
        heroManager.GetHitEvent += OnHit;
        heroManager.DeathEvent += OnDeath;
        heroManager.AttackEvent += OnAttack;
        heroManager.ChangeEvent += OnChange;
        heroManager.ToPositionEvent += OnToPosition;

        m_Anim.runtimeAnimatorController = m_ACSwordShield; // установим АС по умолчанию - щит-меч
        m_Anim.Rebind();                                    // перезапустить АС, чтоб перешел в исходное состояние

        heroManager.InvokeToPositionEvent();                // надеюсь, HeroAudio успел подписаться на это событие...
    }

    // чтобы HeroAudio точно успел во время первого запуска, ибо не факт, что OnEnable HeroAnimator-а запустится раньше оного HeroAudio
    public void Start()
    {
        heroManager.InvokeToPositionEvent();
    }

    private void OnDisable()  
    {
        heroManager.GetHitEvent -= OnHit;
        heroManager.DeathEvent -= OnDeath;
        heroManager.AttackEvent -= OnAttack;
        heroManager.ChangeEvent -= OnChange;
        heroManager.ToPositionEvent -= OnToPosition;
    }

    // Установить начальное положение героя, задать исходное на ристалище 
    public void SetStartPositions(float zeroZposition, float zeroYrotation, float stockXposition, float startRotation)  
    {
        this.zeroZposition = zeroZposition;
        this.zeroYrotation = zeroYrotation;
        this.stockXposition = stockXposition;

        transform.position = new Vector3(stockXposition, 0, zeroZposition);     // установить начальное положение
        transform.rotation = Quaternion.Euler(0f, startRotation, 0f);           // установить начальное вращение
    }

    private void OnHit(int strikeNumber = 0)        // всё равно, какой удар
    {
        m_Anim.SetBool("GetHit", true);                            
    }
    private void OnDeath()
    {
        m_Anim.SetBool("Die", true);
        m_change = false;
    }
    private void OnChange()
    {
        m_change = true;
    }
    private void OnAttack()
    {
        m_Anim.SetBool("Attack", true);                      
    }
    private void OnToPosition()
    {
        m_toPosition = true;
    }

    // Функция плавного поворота до newY [0-360) градусов вокруг оси Y. Возвращает true, если поворот достигнут.
    // Если надо прокрутиться через 0, использовать 2 раза: до 359 и далее... 
    /* 
    Вариант со штатным демпфером - наверное, он хорош, когда мы не знаем угла (и скорости) поворота заранее, а сейчас слишком сложен 
    private float RotationVelocity;                                             //переменная, нужная Mathf.SmoothDampAngle. Задать глобально
    float yAngel = Mathf.SmoothDampAngle(0f, 90f, ref RotationVelocity, 0.3f);  // Рассчитать угол, на который надо повернуться за такт, чтоб на 90 градусов повернуться за 0.3 сек
    Quaternion rotationToStore = Quaternion.Euler(0f, yAngel, 0f);              // Выдать вращение, равное этому углу, относительно оси Y в кватернионе
    transform.rotation *= rotationToStore;                                      // Применить его к текущему вращению
    */
    public bool SmoothRotation(float newY)
    {
        float newYPE = newY + zeroYrotation;                                              // заданное вращение, противоположное для игрока и врага
        float currentY = heroManager.transform.rotation.eulerAngles.y;                    // текущее вращение [0-360 градусов)
        if (currentY == newYPE) return true;                                              // поворот выполнен
        Quaternion newRotation = Quaternion.Euler(0f, angle_speed * Time.deltaTime, 0f);
        if (currentY + newRotation.eulerAngles.y >= 360f)                                 // на этом шаге перешагнем через 0... Все остальное вращение произойдет мигом
        {
            currentY = newYPE;
        }
        heroManager.transform.rotation *= newRotation;                                    // крутимся по часовой
        //transform.rotation *= Quaternion.Inverse(newRotation);                          // так было бы против часовой...
        if (currentY >= newYPE)                                                           // докрутились ли?
        {
            heroManager.transform.rotation = Quaternion.Euler(0f, newYPE + 0.1f, 0f);     // подравнять вращение
            return true;
        }
        else return false;
    }


    //Функция плавного перемещения вдоль оси X. Возвращает true, если нужная позиция достигнута
    /*
    Вариант со штатным демпфером. Наверное, он хорош, когда мы не знаем вектора (и скорости) заранее, а сейчас слишком сложен 
    private Vector3 Velocity = Vector3.zero;                             // эту переменную надо задать глобально
    Vector3 Destination = new Vector3(2.5f, 0, -1.5f);                   // вектор края ристалища (туда бежим на смену оружия)
    transform.position = Vector3.SmoothDamp(transform.position, Destination, ref Velocity, 0.6f);
    */
    public bool SmoothMotion(float X)
    {
        if (heroManager.transform.position.x == X) return true;                                             // перемещение достигнуто
        if (X > heroManager.transform.position.x)                                                           // Если координату X необходимо увеличивать
        {
            Vector3 Destination = new Vector3(X + 0.1f, 0, zeroZposition) - heroManager.transform.position; // то увеличиваем
            heroManager.transform.position += Destination * line_speed * Time.deltaTime;
            if (heroManager.transform.position.x >= X)                                                      // и сравниваем, не стало ли X больше задания
            {
                heroManager.transform.position = new Vector3(X, 0, zeroZposition);                          // подравнять X
                return true;
            }
            else return false;
        }
        else                                                                                                // иначе координату X необходимо уменьшать
        {
            Vector3 Destination = new Vector3(X - 0.1f, 0, zeroZposition) - heroManager.transform.position; // уменьшаем
            heroManager.transform.position += Destination * line_speed * Time.deltaTime;
            if (heroManager.transform.position.x <= X)                                                      // и сравниваем, не стало ли X меньше задания
            {
                heroManager.transform.position = new Vector3(X, 0, zeroZposition);                          // подравнять X
                return true;
            }
            else return false;
        }
    }

    // Разворот после смены оружия
    public void RotateToCenter()
    {
        // a= transform.rotation.eulerAngles.y;             // для отладки  
        if (SmoothRotation(270f))
        {
            m_rotateToCenter = false;                       // сбросим триггер - вращение на центр
            heroManager.InvokeToPositionEvent();
        }
    }

    // Выход на центр ристалища
    public void ToPosition()
    {
        if (SmoothMotion(0f))
        {
            if (SmoothRotation(359.9f)) m_toPosition = false;     // сбрасываем глобальный триггер, когда стоим лицом к врагу                                                      
        }
    }

    // СМЕНА ОРУЖИЯ
    public float line_speed = 1.2f;                               // линейная скорость бега героя
    public float angle_speed = 180f;                              // угловая скорость поворота героя
    public void ChangeWeapon()
    {
        if (SmoothRotation(90f))                                                    // вращение на 90 градусов
        {
            if (!m_Anim.GetBool("Change")) // анимация - запустить 1 раз. "Change" - булева переменная, сама не сбростися. Остальные параметры АС - триггеры, сами ресетятся через такт   
            {
                m_Anim.SetBool("Change", true);
                // можно звук смены начать проигрывать здесь же...
            }
            if (SmoothMotion(stockXposition))                                       // Перемещение к краю ристалища
            {
                m_change = false;                                                   // сбрасываем глобальный триггер 
                m_Anim.SetBool("Change", false);                                    // и анимационный

                switch (heroManager.weaponSet)                   // Отображаем нужный набор оружия и включаем нужный анимационный контроллер 
                {
                    case WeaponSet.SwordShield:
                        heroManager.hero2HandedSword.SetActive(false);
                        heroManager.heroSword_2.SetActive(false);
                        heroManager.heroSword.SetActive(true);
                        heroManager.heroShield.SetActive(true);
                        m_Anim.runtimeAnimatorController = m_ACSwordShield;
                        break;
                    case WeaponSet.SwordSword:
                        heroManager.hero2HandedSword.SetActive(false);
                        heroManager.heroSword_2.SetActive(true);
                        heroManager.heroSword.SetActive(true);
                        heroManager.heroShield.SetActive(false);
                        m_Anim.runtimeAnimatorController = m_ACSwordSword;
                        break;
                    case WeaponSet.TwoHandedSword:
                        heroManager.hero2HandedSword.SetActive(true);
                        heroManager.heroSword_2.SetActive(false);
                        heroManager.heroSword.SetActive(false);
                        heroManager.heroShield.SetActive(false);
                        m_Anim.runtimeAnimatorController = m_AC2HandedSword;
                        break;
                }

                m_rotateToCenter = true;                                     // Взвести триггер - вращение на центр

            }
        }
    }

    private void Update()
    {
        if (m_change && (!heroManager.m_dead)) ChangeWeapon();

        if (m_rotateToCenter) RotateToCenter();

        if (m_toPosition) ToPosition();
    }

}
