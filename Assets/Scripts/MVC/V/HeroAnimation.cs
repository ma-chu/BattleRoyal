using UnityEngine;
//  ДВИЖЕНИЕ И АНИМАЦИЯ ГЕРОЯ
public class HeroAnimation : MonoBehaviour
{
    [SerializeField] private float zeroZposition = 1.35f;      // позиция героя на ристалище - в HEROManager (readonly var)!!! 
    [SerializeField] private float zeroYrotation = -180f;      // вращение героя на ристалище  
    [SerializeField] private float stockXposition = -2.2f;     // начальное вращение героя 
    [SerializeField] private float startRotation = 90f;        // начальная позиция героя (позиция склада)
    
    private HeroManager _heroManager;

    // СОСТОЯНИЯ
    [SerializeField]
    private bool m_change;
    [SerializeField]
    private bool m_toPosition;                           // Выход на ристалище - в начале боя и после смены оружия
    [SerializeField]
    private bool m_rotateToCenter;                       // Разворот после смены оружия

    //private float zeroZposition;                         // координата Z позиции на ристалище 
    //private float zeroYrotation;                         // вращение позиции на ристалище
    //private float stockXposition;                        // координата X позиции склада оружия

    private Animator _anim;                             // ссылка на компонент-аниматор этого героя
    [SerializeField]
    private RuntimeAnimatorController m_ACSwordShield;   // ссылки на контроллеры анимации
    [SerializeField]
    private AnimatorOverrideController m_AC2HandedSword;
    [SerializeField]
    private AnimatorOverrideController m_ACSwordSword;


    private void Awake()
    {
        _heroManager = GetComponent<HeroManager>() /*as HeroManager*/;
        _anim = GetComponentInChildren<Animator>() /*as Animator*/; 
    }

    private void OnEnable()    
    {
        if (_heroManager != null)
        {
            _heroManager.GetHitEvent += OnHit;
            _heroManager.DeathEvent += OnDeath;
            _heroManager.AttackEvent += OnAttack;
            _heroManager.ChangeEvent += OnChange;
            _heroManager.ToPositionEvent += OnToPosition;
        }

        _anim.runtimeAnimatorController = m_ACSwordShield; // установим АС по умолчанию - щит-меч
        _anim.Rebind();                                    // перезапустить АС, чтоб перешел в исходное состояние

        _heroManager.InvokeToPositionEvent();                // надеюсь, HeroAudio успел подписаться на это событие...
    }

    // чтобы HeroAudio точно успел во время первого запуска, ибо не факт, что OnEnable HeroAnimator-а запустится раньше оного HeroAudio
    public void Start()
    {
        _heroManager.InvokeToPositionEvent();
    }

    private void OnDisable()  
    {
        if (_heroManager != null)
        {
            _heroManager.GetHitEvent -= OnHit;
            _heroManager.DeathEvent -= OnDeath;
            _heroManager.AttackEvent -= OnAttack;
            _heroManager.ChangeEvent -= OnChange;
            _heroManager.ToPositionEvent -= OnToPosition;
        }
    }

    // Установить начальное положение героя, задать исходное на ристалище 
    public void SetStartPositions()  
    {
        transform.position = new Vector3(stockXposition, 0, zeroZposition);     // установить начальное положение
        transform.rotation = Quaternion.Euler(0f, startRotation, 0f);           // установить начальное вращение
    }

    private void OnHit(int strikeNumber = 0, int gotDamage = 0)        // всё равно, какой удар и на сколько
    {
        _anim.SetBool("GetHit", true);                            
    }
    private void OnDeath()
    {
        _anim.SetBool("Die", true);
        m_change = false;
    }
    private void OnChange()
    {
        m_change = true;
    }
    private void OnAttack()
    {
        _anim.SetBool("Attack", true);                      
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
        float currentY = _heroManager.transform.rotation.eulerAngles.y;                    // текущее вращение [0-360 градусов)
        if (currentY == newYPE) return true;                                              // поворот выполнен
        Quaternion newRotation = Quaternion.Euler(0f, angleSpeed * Time.deltaTime, 0f);
        if (currentY + newRotation.eulerAngles.y >= 360f)                                 // на этом шаге перешагнем через 0... Все остальное вращение произойдет мигом
        {
            currentY = newYPE;
        }
        _heroManager.transform.rotation *= newRotation;                                    // крутимся по часовой
        //transform.rotation *= Quaternion.Inverse(newRotation);                          // так было бы против часовой...
        if (currentY >= newYPE)                                                           // докрутились ли?
        {
            _heroManager.transform.rotation = Quaternion.Euler(0f, newYPE + 0.1f, 0f);     // подравнять вращение
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
        if (_heroManager.transform.position.x == X) return true;                                             // перемещение достигнуто
        if (X > _heroManager.transform.position.x)                                                           // Если координату X необходимо увеличивать
        {
            Vector3 Destination = new Vector3(X + 0.1f, 0, zeroZposition) - _heroManager.transform.position; // то увеличиваем
            _heroManager.transform.position += Destination * lineSpeed * Time.deltaTime;
            if (_heroManager.transform.position.x >= X)                                                      // и сравниваем, не стало ли X больше задания
            {
                _heroManager.transform.position = new Vector3(X, 0, zeroZposition);                          // подравнять X
                return true;
            }
            else return false;
        }
        else                                                                                                // иначе координату X необходимо уменьшать
        {
            Vector3 Destination = new Vector3(X - 0.1f, 0, zeroZposition) - _heroManager.transform.position; // уменьшаем
            _heroManager.transform.position += Destination * lineSpeed * Time.deltaTime;
            if (_heroManager.transform.position.x <= X)                                                      // и сравниваем, не стало ли X меньше задания
            {
                _heroManager.transform.position = new Vector3(X, 0, zeroZposition);                          // подравнять X
                return true;
            }
            else return false;
        }
    }

    // Разворот после смены оружия
    public void RotateToCenter()
    {
        if (SmoothRotation(270f))
        {
            m_rotateToCenter = false;                       // сбросим триггер - вращение на центр
            _heroManager.InvokeToPositionEvent();
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
    [SerializeField] private float lineSpeed = 1.2f;                               // линейная скорость бега героя
    [SerializeField] float angleSpeed = 180f;                              // угловая скорость поворота героя
    private void ChangeWeapon()
    {
        if (SmoothRotation(90f))                                               // вращение на 90 градусов
        {
            if (!_anim.GetBool("Change")) _anim.SetBool("Change", true);
            if (SmoothMotion(stockXposition))                                       // Перемещение к краю ристалища
            {
                m_change = false;                                                   // сбрасываем глобальный триггер 
                _anim.SetBool("Change", false);                                    // и анимационный

                switch (_heroManager.weaponSet)                   // Отображаем нужный набор оружия и включаем нужный анимационный контроллер 
                {
                    case WeaponSet.SwordShield:
                        _heroManager.hero2HandedSword.SetActive(false);
                        _heroManager.heroSword_2.SetActive(false);
                        _heroManager.heroSword.SetActive(true);
                        _heroManager.heroShield.SetActive(true);
                        _anim.runtimeAnimatorController = m_ACSwordShield;
                        break;
                    case WeaponSet.SwordSword:
                        _heroManager.hero2HandedSword.SetActive(false);
                        _heroManager.heroSword_2.SetActive(true);
                        _heroManager.heroSword.SetActive(true);
                        _heroManager.heroShield.SetActive(false);
                        _anim.runtimeAnimatorController = m_ACSwordSword;
                        break;
                    case WeaponSet.TwoHandedSword:
                        _heroManager.hero2HandedSword.SetActive(true);
                        _heroManager.heroSword_2.SetActive(false);
                        _heroManager.heroSword.SetActive(false);
                        _heroManager.heroShield.SetActive(false);
                        _anim.runtimeAnimatorController = m_AC2HandedSword;
                        break;
                }

                m_rotateToCenter = true;                                     // Взвести триггер - вращение на центр

            }
        }
    }

    private void Update()
    {
        if (m_change && (!_heroManager.dead)) ChangeWeapon();

        if (m_rotateToCenter) RotateToCenter();

        if (m_toPosition) ToPosition();
    }

}
