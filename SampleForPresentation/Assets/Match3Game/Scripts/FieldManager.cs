using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldManager : MonoBehaviour {

    // Поле как двумерный массив целых чисел
    private int[,] field;

    // Поле как двумерный массив фишек
    private GameObject[,] fieldOfGOs;

    // Промежуточное поле для всех модификаций, которое хранит ссылки на fieldOfGOs
    private GameObject[,] tempFieldOfGOs;

    // Размер поля:
    private int fieldSize = 8;

    // Количество фишек
    private int chipCount = 5;

    // Сами фишки (префабы)
    public GameObject[] chipGameObjects;

    // Итераторы, которые отвечают за то, сколько у фишки соседей того же типа
    private int right, left, up, down;

    // Все объекты из комбинаций на удаление
    private List<GameObject> matchingChips;

    // FieldManager находится процессе проверки комбинаций
    public bool isChekingCombination = false;

    // Алгоритм работает и тапы по экрану не обрабатываются
    public bool isWorking = true;

    // Предыдущая нажатая фишка (для проверки на "соседство" с текущей)
    public GameObject previousChip = null;

    // Сдвиги для луча, который ищет соседей. Нужны, т.к. у префабов scale = 1.5
    private Vector3 rightOffset;
    private Vector3 leftOffset;
    private Vector3 downOffset;
    private Vector3 upOffset;

    public static FieldManager instance = null;

    public void Awake()
    {
        chipCount = chipGameObjects.Length;
        field = new int[fieldSize, fieldSize];
        fieldOfGOs = new GameObject[fieldSize, fieldSize];
        tempFieldOfGOs = new GameObject[fieldSize, fieldSize];

        rightOffset = new Vector3(1f, 0f, 0f);
        leftOffset = new Vector3(-1f, 0f, 0f);
        upOffset = new Vector3(0f, 1f, 0f);
        downOffset = new Vector3(0f, -1f, 0f);

        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
    }

    // Случайная генерация числового поля
    private void Generate()
    {
        for (int i = 0; i < fieldSize; i++)
        {
            for (int j = 0; j < fieldSize; j++)
            {
                field[i, j] = UnityEngine.Random.Range(0, chipCount);
            }
        }
    }

    // Первоначальная генерация фишек
    private void CreateChipPrefabs()
    {
        for (int i = 0; i < fieldSize; i++)
        {
            for (int j = 0; j < fieldSize; j++)
            {
                fieldOfGOs[i, j] = Instantiate(chipGameObjects[field[i, j]]);
                fieldOfGOs[i, j].transform.position = new Vector3(i, j, 0f);
                fieldOfGOs[i, j].GetComponent<ParticleSystem>().Stop();

                tempFieldOfGOs[i, j] = fieldOfGOs[i, j];
            }
        }
    }

    /* Основной метод для проверки комбинаций.
     * Так как необходимо учитывать комбинации уголками, то их проверяем в первую очередь.
     * Если из уголков ничего не найдено, то проверяем на наличие линий.
     */
    public IEnumerator CheckCombination()
    {
        isWorking = true;
        isChekingCombination = true;
        do
        {
            yield return new WaitForSeconds(0.2f);
            // Список совпадающих фишек
            matchingChips = new List<GameObject>();

            // Угол вверх и вправо
            for (int i = 0; i < fieldSize; i++)
            {
                for (int j = 0; j < fieldSize; j++)
                {
                    up = 0;
                    right = 0;
                    matchingChips.Clear();

                    List<GameObject> rightMatch = new List<GameObject>();
                    List<GameObject> upMatch = new List<GameObject>();

                    // Если спрайт текущей фишки не null (то есть она является неиспользованной), то начинаем поиск матчей
                    if (tempFieldOfGOs[i, j].GetComponent<SpriteRenderer>().sprite != null)
                    {
                        CheckDirection(i, j, rightMatch, rightOffset, Vector2.right);
                        tempFieldOfGOs[i, j].GetComponent<SpriteRenderer>().sprite = fieldOfGOs[i, j].GetComponent<SpriteRenderer>().sprite;

                        CheckDirection(i, j, upMatch, upOffset, Vector2.up);

                        right = rightMatch.Count;
                        up = upMatch.Count;

                        int m;

                        if (up >= 2 & right == up)
                        {
                            foreach (GameObject chip in upMatch)
                            {
                                matchingChips.Add(chip);
                            }
                            foreach (GameObject chip in rightMatch)
                            {
                                matchingChips.Add(chip);
                            }
                            for (m = j; m <= j + up; m++)
                            {
                                tempFieldOfGOs[i, m].GetComponent<SpriteRenderer>().sprite = null;
                            }
                            for (m = i; m <= i + right; m++)
                            {
                                tempFieldOfGOs[m, j].GetComponent<SpriteRenderer>().sprite = null;
                            }
                            Purse.AddCombo(up + right + 1);
                        }
                        else
                        {
                            matchingChips.Clear();
                        }
                    }

                    foreach (GameObject chip in matchingChips)
                    {
                        // Скрываем каждую фишку из комбинации
                        StartCoroutine(HideMatchingChips(chip));
                    }
                    matchingChips.Clear();
                }
            }
            
            // Угол вверх и влево
            for (int i = fieldSize - 1; i >= 0; i--)
            {
                for (int j = 0; j < fieldSize; j++)
                {
                    up = 0;
                    left = 0;

                    matchingChips.Clear();

                    List<GameObject> leftMatch = new List<GameObject>();
                    List<GameObject> upMatch = new List<GameObject>();

                    // Если спрайт текущей фишки не null (то есть она является неиспользованной), то начинаем поиск матчей
                    if (tempFieldOfGOs[i, j].GetComponent<SpriteRenderer>().sprite != null)
                    {
                        CheckDirection(i, j, upMatch, upOffset, Vector2.up);
                        tempFieldOfGOs[i, j].GetComponent<SpriteRenderer>().sprite = fieldOfGOs[i, j].GetComponent<SpriteRenderer>().sprite;

                        CheckDirection(i, j, leftMatch, leftOffset, Vector2.left);

                        left = leftMatch.Count;
                        up = upMatch.Count;

                        int m;

                        // Четвертая четверть
                        if (left >= 2 & up == left)
                        {
                            foreach (GameObject chip in leftMatch)
                            {
                                matchingChips.Add(chip);
                            }
                            foreach (GameObject chip in upMatch)
                            {
                                matchingChips.Add(chip);
                            }
                            for (m = i - left; m <= i; m++)
                            {
                                tempFieldOfGOs[m, j].GetComponent<SpriteRenderer>().sprite = null;
                            }
                            for (m = j; m <= j + up; m++)
                            {
                                tempFieldOfGOs[i, m].GetComponent<SpriteRenderer>().sprite = null;
                            }
                            Purse.AddCombo(left + up + 1);
                        }
                        else
                        {
                            matchingChips.Clear();
                        }
                    }

                    foreach (GameObject chip in matchingChips)
                    {
                        StartCoroutine(HideMatchingChips(chip));
                    }

                }
            }

            // Угол вниз и вправо
            for (int i = 0; i < fieldSize; i++)
            {
                for (int j = fieldSize - 1; j >= 0; j--)
                {
                    down = 0;
                    right = 0;
                    matchingChips.Clear();

                    List<GameObject> rightMatch = new List<GameObject>();
                    List<GameObject> downMatch = new List<GameObject>();

                    // Если спрайт текущей фишки не null (то есть она является неиспользованной), то начинаем поиск матчей
                    if (tempFieldOfGOs[i, j].GetComponent<SpriteRenderer>().sprite != null)
                    {
                        CheckDirection(i, j, rightMatch, rightOffset, Vector2.right);
                        tempFieldOfGOs[i, j].GetComponent<SpriteRenderer>().sprite = fieldOfGOs[i, j].GetComponent<SpriteRenderer>().sprite;

                        CheckDirection(i, j, downMatch, downOffset, Vector2.down);

                        right = rightMatch.Count;
                        down = downMatch.Count;

                        int m;

                        // Вторая четверть
                        if (right >= 2 & down == right)
                        {
                            foreach (GameObject chip in rightMatch)
                            {
                                matchingChips.Add(chip);
                            }
                            foreach (GameObject chip in downMatch)
                            {
                                matchingChips.Add(chip);
                            }
                            for (m = i; m <= i + right; m++)
                            {
                                tempFieldOfGOs[m, j].GetComponent<SpriteRenderer>().sprite = null;
                            }
                            for (m = j - down; m <= j; m++)
                            {
                                tempFieldOfGOs[i, m].GetComponent<SpriteRenderer>().sprite = null;
                            }
                            Purse.AddCombo(right + down + 1);
                        }
                        else
                        {
                            matchingChips.Clear();
                        }

                        foreach (GameObject chip in matchingChips)
                        {
                            StartCoroutine(HideMatchingChips(chip));
                        }

                    }
                }
            }

            // Угол вниз и влево
            for (int i = fieldSize - 1; i >= 0; i--)
            {
                for (int j = fieldSize - 1; j >= 0; j--)
                {
                    down = 0;
                    left = 0;
                    matchingChips.Clear();

                    List<GameObject> leftMatch = new List<GameObject>();
                    List<GameObject> downMatch = new List<GameObject>();

                    // Если спрайт текущей фишки не null (то есть она является неиспользованной), то начинаем поиск матчей
                    if (tempFieldOfGOs[i, j].GetComponent<SpriteRenderer>().sprite != null)
                    {
                        CheckDirection(i, j, downMatch, downOffset, Vector2.down);
                        tempFieldOfGOs[i, j].GetComponent<SpriteRenderer>().sprite = fieldOfGOs[i, j].GetComponent<SpriteRenderer>().sprite;

                        CheckDirection(i, j, leftMatch, leftOffset, Vector2.left);

                        left = leftMatch.Count;
                        down = downMatch.Count;

                        int m;

                        // Третья четверть
                        if (down >= 2 & left == down)
                        {
                            foreach (GameObject chip in downMatch)
                            {
                                matchingChips.Add(chip);
                            }
                            foreach (GameObject chip in leftMatch)
                            {
                                matchingChips.Add(chip);
                            }
                            for (m = j - down; m <= j; m++)
                            {
                                tempFieldOfGOs[i, m].GetComponent<SpriteRenderer>().sprite = null;
                            }
                            for (m = i - left; m <= i; m++)
                            {
                                tempFieldOfGOs[m, j].GetComponent<SpriteRenderer>().sprite = null;
                            }
                            Purse.AddCombo(down + left + 1);
                        }
                        else
                        {
                            matchingChips.Clear();
                        }
                    }

                    foreach (GameObject chip in matchingChips)
                    {
                        StartCoroutine(HideMatchingChips(chip));
                    }

                    matchingChips.Clear();

                }
            }

            // Линия вверх
            for (int i = 0; i < fieldSize; i++)
            {
                for (int j = 0; j < fieldSize; j++)
                {
                    up = 0;
                    matchingChips.Clear();

                    List<GameObject> upMatch = new List<GameObject>();

                    // Если спрайт текущей фишки не null (то есть она является неиспользованной), то начинаем поиск матчей
                    if (tempFieldOfGOs[i, j].GetComponent<SpriteRenderer>().sprite != null)
                    {
                        CheckDirection(i, j, upMatch, upOffset, Vector2.up);
                        tempFieldOfGOs[i, j].GetComponent<SpriteRenderer>().sprite = fieldOfGOs[i, j].GetComponent<SpriteRenderer>().sprite;

                        up = upMatch.Count;

                        int m;

                        if (up >= 2)
                        {
                            foreach (GameObject chip in upMatch)
                            {
                                matchingChips.Add(chip);
                            }
                            for (m = j; m <= j + up; m++)
                            {
                                tempFieldOfGOs[i, m].GetComponent<SpriteRenderer>().sprite = null;
                            }
                            Purse.AddCombo(up + 1);
                        }
                    }

                    foreach (GameObject chip in matchingChips)
                    {
                        StartCoroutine(HideMatchingChips(chip));
                    }

                    matchingChips.Clear();

                }
            }

            // Линия вправо
            for (int i = 0; i < fieldSize; i++)
            {
                for (int j = 0; j < fieldSize; j++)
                {
                    right = 0;
                    matchingChips.Clear();

                    List<GameObject> rightMatch = new List<GameObject>();

                    // Если спрайт текущей фишки не null (то есть она является неиспользованной), то начинаем поиск матчей
                    if (tempFieldOfGOs[i, j].GetComponent<SpriteRenderer>().sprite != null)
                    {
                        CheckDirection(i, j, rightMatch, rightOffset, Vector2.right);
                        tempFieldOfGOs[i, j].GetComponent<SpriteRenderer>().sprite = fieldOfGOs[i, j].GetComponent<SpriteRenderer>().sprite;

                        right = rightMatch.Count;

                        int m;

                        if (right >= 2)
                        {
                            foreach (GameObject chip in rightMatch)
                            {
                                matchingChips.Add(chip);
                            }
                            for (m = i; m <= i + right; m++)
                            {
                                tempFieldOfGOs[m, j].GetComponent<SpriteRenderer>().sprite = null;
                            }
                            Purse.AddCombo(right + 1);
                        }
                    }

                    foreach (GameObject chip in matchingChips)
                    {
                        StartCoroutine(HideMatchingChips(chip));
                    }

                    matchingChips.Clear();
                }
            }

            yield return new WaitForSeconds(0.01f);
            StartCoroutine(MoveChipsDown());
            isChekingCombination = false;
        }
        while (isChekingCombination);
    }

    // Прячем фишки, которые вошли в комбинацию
    private IEnumerator HideMatchingChips(GameObject chip)
    {
        ParticleSystem ps = chip.GetComponent<ParticleSystem>();
        ps.Play();
        chip.GetComponent<SpriteRenderer>().sprite = null;
        yield return new WaitForSeconds(0.05f);
        ps.Stop();
    }

    // Запускаем процедуру сдвига фишек вниз
    private IEnumerator MoveChipsDown()
    {
        for (int i = 0; i < fieldSize; i++)
        {
            for (int j = 0; j < fieldSize; j++)
            {
                if (tempFieldOfGOs[i, j].GetComponent<SpriteRenderer>().sprite == null)
                {
                    yield return StartCoroutine(MoveDown(i, j));
                    break;
                }
            }
        }
        StartCoroutine(FillEmptySpaces());
    }

    // Если есть что сдвигать, то сдвигаем вниз
    private IEnumerator MoveDown(int iPos, int jPos)
    {
        int nullCount = 0;

        // Вычисляем, сколько раз надо будет сдвинуть
        for (int y = jPos; y < fieldSize; y++)
        {
            SpriteRenderer renderer = tempFieldOfGOs[iPos, y].GetComponent<SpriteRenderer>();
            if (renderer.sprite == null)
            {
                nullCount++;
            }

        }

        // Фишка null "всплывает" вверх
        for (int j = 0; j < nullCount; j++)
        {
            yield return new WaitForSeconds(0.01f);
            for (int k = 0; k < fieldSize - 1; k++)
            {
                if (tempFieldOfGOs[iPos, k].GetComponent<SpriteRenderer>().sprite == null)
                { 
                    tempFieldOfGOs[iPos, k].GetComponent<SpriteRenderer>().sprite = tempFieldOfGOs[iPos, k + 1].GetComponent<SpriteRenderer>().sprite;
                    tempFieldOfGOs[iPos, k + 1].GetComponent<SpriteRenderer>().sprite = null;

                }
            }
        }
    }

    // Заполняем пустые места, оставшиеся после "сокращения" фишек
    private IEnumerator FillEmptySpaces()
    {
        int nullCount = 0;
        for (int j = 0; j < fieldSize; j++)
        {
            for (int i = 0; i < fieldSize; i++)
            {
                SpriteRenderer renderer = tempFieldOfGOs[i, j].GetComponent<SpriteRenderer>();
                if (renderer.sprite == null)
                {
                    nullCount++;
                    renderer.sprite = chipGameObjects[UnityEngine.Random.Range(0, chipCount)].GetComponent<SpriteRenderer>().sprite;
                    yield return new WaitForSeconds(0.01f);
                }
            }
        }
        if (nullCount > 0)
            StartCoroutine(CheckCombination());
        else
            isWorking = false;
    }

    // Проверка, какая фишка находится в заданном направлении direction и добавление ее в список соседей match
    private void CheckDirection(int i, int j, List<GameObject> match, Vector3 offset, Vector2 direction)
    {
        if (tempFieldOfGOs[i, j].GetComponent<SpriteRenderer>().sprite != null)
        {

            GameObject chip = tempFieldOfGOs[i, j];

            Sprite sprite = chip.GetComponent<SpriteRenderer>().sprite;
            RaycastHit2D hit = Physics2D.Raycast(chip.transform.position + offset, direction);

            // Луч задел какой-то объект
            if (hit.collider != null)
            {
                // Спрайт задетого объекта совпадает со спрайтом текущей фишки
                if (hit.collider.GetComponent<SpriteRenderer>().sprite == sprite)
                {
                    match.Add(hit.collider.gameObject);

                    if (direction == Vector2.down)
                    {
                        if (j - 1 >= 0)
                            CheckDirection(i, j - 1, match, new Vector3(0f, -1f, 0f), direction);
                    }
                    else if (direction == Vector2.up)
                    {
                        if (j <= fieldSize + 1)
                            CheckDirection(i, j + 1, match, new Vector3(0f, 1f, 0f), direction);
                    }
                    else if (direction == Vector2.right)
                    {
                        if (i + 1 <= fieldSize)
                            CheckDirection(i + 1, j, match, new Vector3(1f, 0f, 0f), direction);
                    }
                    else if (direction == Vector2.left)
                    {
                        if (i - 1 >= 0)
                            CheckDirection(i - 1, j, match, new Vector3(-1f, 0f, 0f), direction);
                    }
                }
            }

        }
    }

    // Скрыть все фишки
    public void HideAllChips()
    {
        for (int j = 0; j < fieldSize; j++)
        {
            for (int i = 0; i < fieldSize; i++)
            {
                tempFieldOfGOs[i, j].SetActive(false);
            }
        }
    }

    public void Start()
    {
        StartGame();
    }

    // Запуск игры
    public void StartGame()
    {
        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();
        Generate();
        CreateChipPrefabs();
        StartCoroutine(CheckCombination());
        stopwatch.Stop();
        TimeSpan ts = stopwatch.Elapsed;
        Debug.Log(ts);
    }
}
