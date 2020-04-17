# Вывод математики для метода FDTD

## Уравнения Максвелла для связи

### Теорема Стокса

$rot\vec H=\frac{\partial \vec D}{\partial t}$

$rot\vec E=-\frac{\partial \vec B}{\partial t}$

### Уравнения связи

Электрическая индукция

$\vec D=\varepsilon\varepsilon_0 \vec E$

Магнитная индукция

$\vec B=\mu\mu_0 \vec H$

## Рзложение ротора

$
rot\vec H=
\left | \left.\begin{matrix}
i & j & k
\\ 
\frac{\partial}{\partial x} & \frac{\partial}{\partial y} & \frac{\partial}{\partial z}
\\ 
H_x & H_y & H_z
\end{matrix}\right| \right .
$$=\begin{bmatrix}
\frac{\partial H_z}{\partial y} - \frac{\partial H_y}{\partial z}
\\ 
\frac{\partial H_x}{\partial z} - \frac{\partial H_z}{\partial x}
\\ 
\frac{\partial H_y}{\partial x} - \frac{\partial H_x}{\partial y}
\end{bmatrix}
$

$
rot\vec E=
\left | \left.\begin{matrix}
i & j & k\\ 
\frac{\partial}{\partial x} & \frac{\partial}{\partial y} & \frac{\partial}{\partial z}\\ 
E_x & E_y & E_z
\end{matrix}\right| \right .
$$=\begin{bmatrix}
\frac{\partial E_z}{\partial y} - \frac{\partial E_y}{\partial z}
\\ 
\frac{\partial E_x}{\partial z} - \frac{\partial E_z}{\partial x}
\\ 
\frac{\partial E_y}{\partial x} - \frac{\partial E_x}{\partial y}
\end{bmatrix}
$

## Переход к конечным разностям

### Дифференциал по времени
Определяется как разность между значением в текущий момент времени и значением, которое было на предыдущем шаге. Разность нормируется к величине дискрета времени $\Delta t$:

$\frac{\partial D}{\partial t}=\varepsilon\varepsilon_0\frac{E^t-E^{t-\Delta t}}{\Delta t}$

$\frac{\partial B}{\partial t}=\mu\mu_0\frac{H^t-H^{t-\Delta t}}{\Delta t}$

### Дифференциал по пространственной координате
Пространство разбито на ячейки в трёхмерной системе координат (индексов). Дифференциал по пространственной координате заменяется разностью между двумя соседними ячейками с изменением соответствющего координате индекса:
* Координата x - индекс i
* Координата y - индекс j
* Координата z - индекс k

Конечная разность определяется как разность между двумя соседними ячейками по выбранному индексу, делёная на величину пространсвтенного шага (размер ячейки в в заданном направлении):

$\frac{\partial H_z}{\partial y}[i,j,k]=\frac{H_z[i,j,k]-H_z[i,j-1,k]}{\Delta y}$

$\frac{\partial E_z}{\partial y}[i,j,k]=\frac{E_z[i,j+1,k]-E_z[i,j,k]}{\Delta y}$

## Компоненты векторов полей

### Электрическое поле

Перепишем выражение так, чтобы слева было приращение по времени. Ротор магнитного поля приводит к изменению во времени электрической индукции (электрического поля)

$\frac{\partial \vec D}{\partial t}=rot\vec H$

Разложим вектора на компоненты и развернём индукцию

$
\frac{\partial E_x}{\partial t} =
    \frac{1}{\varepsilon\varepsilon_0}
    \left (
        \frac{\partial H_z}{\partial y} 
      - \frac{\partial H_y}{\partial z}
    \right )
$

$
\frac{\partial E_y}{\partial t} =
    \frac{1}{\varepsilon\varepsilon_0}
    \left (
        \frac{\partial H_x}{\partial z} 
      - \frac{\partial H_z}{\partial x}
    \right )
$

$
\frac{\partial E_z}{\partial t} =
    \frac{1}{\varepsilon\varepsilon_0}
    \left (
        \frac{\partial H_y}{\partial x} 
      - \frac{\partial H_x}{\partial y}
    \right )
$

#### Конечные разности

$
\frac{E_x^t-E_x^{t-\Delta t}}{\Delta t}[i,j,k] =
    \frac{1}{\varepsilon\varepsilon_0}
    \left (
        \frac{H_z[i,j,k] - H_z[i,j-1,k]}{\Delta y} 
      - \frac{H_y[i,j,k] - H_y[i,j,k-1]}{\Delta z}
    \right )
$

$
\frac{E_y^t-E_y^{t-\Delta t}}{\Delta t}[i,j,k] =
    \frac{1}{\varepsilon\varepsilon_0}
    \left (
        \frac{H_x[i,j,k] - H_x[i,j,k-1]}{\Delta z} 
      - \frac{H_z[i,j,k] - H_z[i-1,j,k]}{\Delta x}
    \right )
$

$
\frac{E_z^t-E_z^{t-\Delta t}}{\Delta t}[i,j,k] =
    \frac{1}{\varepsilon\varepsilon_0}
    \left (
        \frac{H_y[i,j,k] - H_y[i-1,j,k]}{\Delta x} 
      - \frac{H_x[i,j,k] - H_x[i,j-1,k]}{\Delta y}
    \right )
$

#### Приращения

$E_x[i,j,k] = E_x[i,j,k] +
    \frac{\Delta t}{\varepsilon\varepsilon_0}
    \left (
        \frac{H_z[i,j,k] - H_z[i,j-1,k]}{\Delta y} 
      - \frac{H_y[i,j,k] - H_y[i,j,k-1]}{\Delta z}
    \right )
$

$E_y[i,j,k] = E_y[i,j,k] +
    \frac{\Delta t}{\varepsilon\varepsilon_0}
    \left (
        \frac{H_x[i,j,k] - H_x[i,j,k-1]}{\Delta z} 
      - \frac{H_z[i,j,k] - H_z[i-1,j,k]}{\Delta x}
    \right )
$

$E_z[i,j,k]=E_z[i,j,k] +
    \frac{\Delta t}{\varepsilon\varepsilon_0}
    \left (
        \frac{H_y[i,j,k] - H_y[i-1,j,k]}{\Delta x} 
      - \frac{H_x[i,j,k] - H_x[i,j-1,k]}{\Delta y}
    \right )
$

#### Конечные приращения

$E_x[i,j,k]$+=$
    \frac{\Delta t}{\varepsilon\varepsilon_0}
    \left (
        \frac{H_z[i,j,k] - H_z[i,j-1,k]}{\Delta y} 
      - \frac{H_y[i,j,k] - H_y[i,j,k-1]}{\Delta z}
    \right )
$

$E_y[i,j,k]$+=$
    \frac{\Delta t}{\varepsilon\varepsilon_0}
    \left (
        \frac{H_x[i,j,k] - H_x[i,j,k-1]}{\Delta z} 
      - \frac{H_z[i,j,k] - H_z[i-1,j,k]}{\Delta x}
    \right )
$

$E_z[i,j,k]$+=$
    \frac{\Delta t}{\varepsilon\varepsilon_0}
    \left (
        \frac{H_y[i,j,k] - H_y[i-1,j,k]}{\Delta x} 
      - \frac{H_x[i,j,k] - H_x[i,j-1,k]}{\Delta y}
    \right )
$

### Магнитное поле

$\frac{\partial \vec B}{\partial t}=-rot\vec E$

Разложим вектора на компоненты и развернём индукцию

$
\frac{\partial H_x}{\partial t} = -
    \frac{1}{\mu\mu_0}
    \left (
        \frac{\partial E_z}{\partial y} 
      - \frac{\partial E_y}{\partial z}
    \right )
$

$
\frac{\partial H_y}{\partial t} = -
    \frac{1}{\mu\mu_0}
    \left (
        \frac{\partial E_x}{\partial z} 
        - \frac{\partial E_z}{\partial x}
    \right ) 
$

$
\frac{\partial H_z}{\partial t} = -
    \frac{1}{\mu\mu_0}
    \left (
        \frac{\partial E_y}{\partial x} 
        - \frac{\partial E_x}{\partial y}
    \right )
$

#### Конечные разности

$
\frac{H_x^t-H_x^{t-\Delta t}}{\Delta t}[i,j,k] =
   -\frac{1}{\mu\mu_0}
    \left (
        \frac{E_z[i,j+1,k] - E_z[i,j,k]}{\Delta y} 
      - \frac{E_y[i,j,k+1] - E_y[i,j,k]}{\Delta z}
    \right )
$

$
\frac{H_y^t-H_y^{t-\Delta t}}{\Delta t}[i,j,k] =
   -\frac{1}{\mu\mu_0}
    \left (
        \frac{E_x[i,j,k+1] - E_x[i,j,k]}{\Delta z} 
      - \frac{E_z[i+1,j,k] - E_z[i,j,k]}{\Delta x}
    \right )
$

$
\frac{H_z^t-H_z^{t-\Delta t}}{\Delta t}[i,j,k] =
   -\frac{1}{\mu\mu_0}
    \left (
        \frac{E_y[i+1,j,k] - E_y[i,j,k]}{\Delta x} 
      - \frac{E_x[i,j+1,k] - E_x[i,j,k]}{\Delta y}
    \right )
$

#### Приращения

$H_x[i,j,k] = H_x[i,j,k] -
    \frac{\Delta t}{\mu\mu_0}
    \left (
        \frac{E_z[i,j+1,k] - E_z[i,j,k]}{\Delta y} 
      - \frac{E_y[i,j,k+!] - E_y[i,j,k]}{\Delta z}
    \right )
$

$H_y[i,j,k] = H_y[i,j,k] -
    \frac{\Delta t}{\mu\mu_0}
    \left (
        \frac{E_x[i,j,k+1] - E_x[i,j,k]}{\Delta z} 
      - \frac{E_z[i+1,j,k] - E_z[i,j,k]}{\Delta x}
    \right )
$

$H_z[i,j,k] = H_z[i,j,k] -
    \frac{\Delta t}{\mu\mu_0}
    \left (
        \frac{E_y[i+,j,k] - E_y[i,j,k]}{\Delta x} 
      - \frac{E_x[i,j+1,k] - E_x[i,j,k]}{\Delta y}
    \right )
$

#### Конечные приращения

$H_x[i,j,k]$-=$
    \frac{\Delta t}{\mu\mu_0}
    \left (
        \frac{E_z[i,j+1,k] - E_z[i,j,k]}{\Delta y} 
      - \frac{E_y[i,j,k+1] - E_y[i,j,k]}{\Delta z}
    \right )
$

$H_y[i,j,k]$-=$
    \frac{\Delta t}{\mu\mu_0}
    \left (
        \frac{E_x[i,j,k+1] - E_x[i,j,k]}{\Delta z} 
      - \frac{E_z[i+1,j,k] - E_z[i,j,k]}{\Delta x}
    \right )
$

$H_z[i,j,k]$-=$
    \frac{\Delta t}{\mu\mu_0}
    \left (
        \frac{E_y[i+,j,k] - E_y[i,j,k]}{\Delta x} 
      - \frac{E_x[i,j+1,k] - E_x[i,j,k]}{\Delta y}
    \right )
$

## В векторной форме

$
\vec E[i,j,k]=\begin{bmatrix}
E_x
\\
E_y
\\
E_z
\end{bmatrix}[i,j,k]
$ += $\frac{\Delta t}{\varepsilon\varepsilon_0}
\begin{bmatrix}
    \frac{H_z[i,j,k] - H_z[i,j-1,k]}{\Delta y} - \frac{H_y[i,j,k] - H_y[i,j,k-1]}{\Delta z}
\\
    \frac{H_x[i,j,k] - H_x[i,j,k-1]}{\Delta z} - \frac{H_z[i,j,k] - H_z[i-1,j,k]}{\Delta x}
\\
    \frac{H_y[i,j,k] - H_y[i-1,j,k]}{\Delta x} - \frac{H_x[i,j,k] - H_x[i,j-1,k]}{\Delta y}
\end{bmatrix}
$

$
\vec H[i,j,k]=\begin{bmatrix}
H_x
\\
H_y
\\
H_z
\end{bmatrix}[i,j,k]
$ -= $\frac{\Delta t}{\mu\mu_0}
\begin{bmatrix}
    \frac{E_z[i,j+1,k] - E_z[i,j,k]}{\Delta y} - \frac{E_y[i,j,k+1] - E_y[i,j,k]}{\Delta z}
\\
    \frac{E_x[i,j,k+1] - E_x[i,j,k]}{\Delta z} - \frac{E_z[i+1,j,k] - E_z[i,j,k]}{\Delta x}
\\
    \frac{E_y[i+,j,k] - E_y[i,j,k]}{\Delta x} - \frac{E_x[i,j+1,k] - E_x[i,j,k]}{\Delta y}
\end{bmatrix}
$

## Частные случаи

### Двумерное пространство

#### Плоскость XOY
Приращение $\Delta z$ отсутствует (удаляем разность по индексу k):

$
\vec E[i,j]=\begin{bmatrix}
E_x
\\
E_y
\\
E_z
\end{bmatrix}[i,j]
$ += $\frac{\Delta t}{\varepsilon\varepsilon_0}
\begin{bmatrix}
    \frac{H_z[i,j] - H_z[i,j-1]}{\Delta y}
\\
   -\frac{H_z[i,j] - H_z[i-1,j]}{\Delta x}
\\
    \frac{H_y[i,j] - H_y[i-1,j]}{\Delta x} - \frac{H_x[i,j] - H_x[i,j-1]}{\Delta y}
\end{bmatrix}
$

$
\vec H[i,j]=\begin{bmatrix}
H_x
\\
H_y
\\
H_z
\end{bmatrix}[i,j]
$ -= $\frac{\Delta t}{\mu\mu_0}
\begin{bmatrix}
    \frac{E_z[i,j+1] - E_z[i,j]}{\Delta y}
\\
   -\frac{E_z[i+1,j] - E_z[i,j]}{\Delta x}
\\
    \frac{E_y[i+1,j] - E_y[i,j]}{\Delta x} - \frac{E_x[i,j+1] - E_x[i,j]}{\Delta y}
\end{bmatrix}
$

#### Плоскость XOZ

Приращение $\Delta y$ отсутствует (удаляем разность по индексу j):

$
\vec E[i,k]=\begin{bmatrix}
E_x
\\
E_y
\\
E_z
\end{bmatrix}[i,k]
$ += $\frac{\Delta t}{\varepsilon\varepsilon_0}
\begin{bmatrix}
   -\frac{H_y[i,j,k] - H_y[i,j,k-1]}{\Delta z}
\\
    \frac{H_x[i,k] - H_x[i,k-1]}{\Delta z} - \frac{H_z[i,k] - H_z[i-1,k]}{\Delta x}
\\
    \frac{H_y[i,k] - H_y[i-1,k]}{\Delta x}
\end{bmatrix}
$

$
\vec H[i,k]=\begin{bmatrix}
H_x
\\
H_y
\\
H_z
\end{bmatrix}[i,k]
$ -= $\frac{\Delta t}{\mu\mu_0}
\begin{bmatrix}
   -\frac{E_y[i,j,k+1] - E_y[i,k]}{\Delta z}
\\
    \frac{E_x[i,k+1] - E_x[i,k]}{\Delta z} - \frac{E_z[i+1,k] - E_z[i,j,k]}{\Delta x}
\\
    \frac{E_y[i+1,k] - E_y[i,k]}{\Delta x}
\end{bmatrix}
$

#### Плоскость YOZ

Приращение $\Delta x$ отсутствует (удаляем разность по индексу i):

$
\vec E[j,k]=\begin{bmatrix}
E_x
\\
E_y
\\
E_z
\end{bmatrix}[j,k]
$ += $\frac{\Delta t}{\varepsilon\varepsilon_0}
\begin{bmatrix}
    \frac{H_z[j,k] - H_z[j-1,k]}{\Delta y} - \frac{H_y[j,k] - H_y[j,k-1]}{\Delta z}
\\
    \frac{H_x[j,k] - H_x[j,k-1]}{\Delta z}
\\
   -\frac{H_x[j,k] - H_x[j-1,k]}{\Delta y}
\end{bmatrix}
$

$
\vec H[j,k]=\begin{bmatrix}
H_x
\\
H_y
\\
H_z
\end{bmatrix}[j,k]
$ -= $\frac{\Delta t}{\mu\mu_0}
\begin{bmatrix}
    \frac{E_z[j+1,k] - E_z[j,k]}{\Delta y} - \frac{E_y[j,k+1] - E_y[j,k]}{\Delta z}
\\
    \frac{E_x[j,k+1] - E_x[j,k]}{\Delta z}
\\
   -\frac{E_x[j+1,k] - E_x[j,k]}{\Delta y}
\end{bmatrix}
$

### Одномерное пространство

#### Ось OX

Исключаем приращения \$Delta y$ и $\Delta z$ (индексы j и k):

$
\begin{bmatrix}
E_x
\\
E_y
\\
E_z
\end{bmatrix}[i]
$ += $\frac{1}{\varepsilon\varepsilon_0}\frac{\Delta t}{\Delta x}
\begin{bmatrix}
    0
\\
    H_z[i-1] - H_z[i]
\\
    H_y[i] - H_y[i-1]
\end{bmatrix}
$

$
\begin{bmatrix}
H_x
\\
H_y
\\
H_z
\end{bmatrix}[i]
$ -= $\frac{1}{\mu\mu_0}\frac{\Delta t}{\Delta x}
\begin{bmatrix}
    0
\\
    E_z[i] - E_z[i+1]
\\
    E_y[i+1] - E_y[i]
\end{bmatrix}
$

#### Ось OY

Исключаем приращения $\Delta x$ и $\Delta z$ (индексы i и k):

$
\begin{bmatrix}
E_x
\\
E_y
\\
E_z
\end{bmatrix}[j]
$ += $\frac{1}{\varepsilon\varepsilon_0}\frac{\Delta t}{\Delta y}
\begin{bmatrix}
    H_z[j] - H_z[j-1]
\\
    0
\\
    H_x[j-1] - H_x[j]
\end{bmatrix}
$

$
\begin{bmatrix}
H_x
\\
H_y
\\
H_z
\end{bmatrix}[j]
$ -= $\frac{1}{\mu\mu_0}\frac{\Delta t}{\Delta y}
\begin{bmatrix}
    E_z[j+1] - E_z[j]
\\
    0
\\
    E_x[j] - E_x[j+1]
\end{bmatrix}
$

#### Ось OZ

Исключаем приращения $\Delta x$ и $\Delta y$ (индексы i и j):

$
\begin{bmatrix}
E_x
\\
E_y
\\
E_z
\end{bmatrix}[k]
$ += $\frac{1}{\varepsilon\varepsilon_0}\frac{\Delta t}{\Delta z}
\begin{bmatrix}
    H_y[k-1] - H_y[k]
\\
    H_x[k] - H_x[k-1]
\\
    0
\end{bmatrix}
$

$
\begin{bmatrix}
H_x
\\
H_y
\\
H_z
\end{bmatrix}[k]
$ -= $\frac{1}{\mu\mu_0}\frac{\Delta t}{\Delta z}
\begin{bmatrix}
    E_y[k] - E_y[k+1]
\\
    E_x[k+1] - E_x[k]
\\
    0
\end{bmatrix}
$

### T-волна вдоль OX (одномерный случай для компонент Ey, Hz)

$
\begin{bmatrix}
E_x
\\
E_y
\\
E_z
\end{bmatrix}[i]
$ += $\frac{1}{\varepsilon\varepsilon_0}\frac{\Delta t}{\Delta x}
\begin{bmatrix}
    0
\\
    H_z[i-1] - H_z[i]
\\
    0
\end{bmatrix}
$

$
\begin{bmatrix}
H_x
\\
H_y
\\
H_z
\end{bmatrix}[i]
$ -= $\frac{1}{\mu\mu_0}\frac{\Delta t}{\Delta x}
\begin{bmatrix}
    0
\\
    0
\\
    E_y[i+1] - E_y[i]
\end{bmatrix}
$

$E_y[i]+=\frac{1}{\varepsilon\varepsilon_0}\frac{\Delta t}{\Delta x}(H_z[i-1] - H_z[i])$

$H_z[i]+=\frac{1}{\mu\mu_0}\frac{\Delta t}{\Delta x}(E_y[i] - E_y[i+1])$