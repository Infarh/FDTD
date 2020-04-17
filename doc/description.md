# Вывод математики для метода FDTD

## Уравнения Максвелла для связи

### Теорема Стокса

$rot\vec\mathbf H=\frac{\partial \vec\mathbf D}{\partial t}$

$rot\vec\mathbf E=-\frac{\partial \vec\mathbf B}{\partial t}$

### Уравнения связи

Электрическая индукция

$\vec\mathbf D=\varepsilon\varepsilon_0 \vec\mathbf E$

Магнитная индукция

$\vec\mathbf B=\mu\mu_0 \vec\mathbf H$

## Рзложение ротора

$
rot\vec\mathbf H=
\left | \left.\begin{matrix}
i & j & k\\ 
\frac{\partial}{\partial x} & \frac{\partial}{\partial y} & \frac{\partial}{\partial z}\\ 
\mathbf H_x & \mathbf H_y & \mathbf H_z
\end{matrix}\right| \right .
$
$=\begin{bmatrix}
\frac{\partial \mathbf H_z}{\partial y} - \frac{\partial \mathbf H_y}{\partial z}
\\ 
\frac{\partial \mathbf H_x}{\partial z} - \frac{\partial \mathbf H_z}{\partial x}
\\ 
\frac{\partial \mathbf H_y}{\partial x} - \frac{\partial \mathbf H_x}{\partial y}
\end{bmatrix}
$

$
rot\vec\mathbf E=
\left | \left.\begin{matrix}
i & j & k\\ 
\frac{\partial}{\partial x} & \frac{\partial}{\partial y} & \frac{\partial}{\partial z}\\ 
\mathbf E_x & \mathbf E_y & \mathbf E_z
\end{matrix}\right| \right .
$
$=\begin{bmatrix}
\frac{\partial \mathbf E_z}{\partial y} - \frac{\partial \mathbf E_y}{\partial z}
\\ 
\frac{\partial \mathbf E_x}{\partial z} - \frac{\partial \mathbf E_z}{\partial x}
\\ 
\frac{\partial \mathbf E_y}{\partial x} - \frac{\partial \mathbf E_x}{\partial y}
\end{bmatrix}
$

## Переход к конечным разностям

### Дифференциал по времени
Определяется как разность между значением в текущий момент времени и значением, которое было на предыдущем шаге. Разность нормируется к величине дискрета времени $dt$:

$\frac{\partial \mathbf D}{\partial t}=\varepsilon\varepsilon_0\frac{E^t-E^{t-dt}}{dt}$

$\frac{\partial \mathbf B}{\partial t}=\mu\mu_0\frac{H^t-H^{t-dt}}{dt}$

### Дифференциал по пространственной координате
Пространство разбито на ячейки в трёхмерной системе координат (индексов). Дифференциал по пространственной координате заменяется разностью между двумя соседними ячейками с изменением соответствющего координате индекса:
* Координата x - индекс i
* Координата y - индекс j
* Координата z - индекс k

Конечная разность определяется как разность между двумя соседними ячейками по выбранному индексу, делёная на величину пространсвтенного шага (размер ячейки в в заданном направлении):
$\frac{\partial \mathbf H_z}{\partial y}[i,j,k]=\frac{H_z[i,j,k]-H_z[i,j-1,k]}{dy}$

## Компоненты векторов полей

### Электрическое поле

Перепишем выражение так, чтобы слева было приращение по времени. Ротор магнитного поля приводит к изменению во времени электрической индукции (электрического поля)

$\frac{\partial \vec\mathbf D}{\partial t}=rot\vec\mathbf H$

Разложим вектора на компоненты и развернём индукцию

$
\varepsilon\varepsilon_0\frac{\partial \mathbf E_x}{\partial t}=
    \frac{\partial \mathbf H_z}{\partial y} - \frac{\partial \mathbf H_y}{\partial z}
$

$
\varepsilon\varepsilon_0\frac{\partial \mathbf E_y}{\partial t}=
    \frac{\partial \mathbf H_x}{\partial z} - \frac{\partial \mathbf H_z}{\partial x}
$

$
\varepsilon\varepsilon_0\frac{\partial \mathbf E_z}{\partial t}=
    \frac{\partial \mathbf H_y}{\partial x} - \frac{\partial \mathbf H_x}{\partial y}
$

#### Конечные разности

$
\frac{E_x^t-E_x^{t-dt}}{dt}[i,j,k] =
    \frac{1}{\varepsilon\varepsilon_0}
    \left (
        \frac{H_z[i,j,k] - H_z[i,j-1,k]}{dy} 
      - \frac{H_y[i,j,k] - H_y[i,j,k-1]}{dz}
    \right )
$

$
\frac{E_y^t-E_y^{t-dt}}{dt}[i,j,k] =
    \frac{1}{\varepsilon\varepsilon_0}
    \left (
        \frac{H_x[i,j,k] - H_x[i,j,k-1]}{dz} 
      - \frac{H_z[i,j,k] - H_z[i-1,j,k]}{dx}
    \right )
$

$
\frac{E_z^t-E_z^{t-dt}}{dt}[i,j,k] =
    \frac{1}{\varepsilon\varepsilon_0}
    \left (
        \frac{H_y[i,j,k] - H_y[i-1,j,k]}{dx} 
      - \frac{H_x[i,j,k] - H_x[i,j-1,k]}{dy}
    \right )
$

#### Приращения

$E_x[i,j,k] = E_x[i,j,k] +
    \frac{dt}{\varepsilon\varepsilon_0}
    \left (
        \frac{H_z[i,j,k] - H_z[i,j-1,k]}{dy} 
      - \frac{H_y[i,j,k] - H_y[i,j,k-1]}{dz}
    \right )
$

$E_y[i,j,k] = E_y[i,j,k] +
    \frac{dt}{\varepsilon\varepsilon_0}
    \left (
        \frac{H_x[i,j,k] - H_x[i,j,k-1]}{dz} 
      - \frac{H_z[i,j,k] - H_z[i-1,j,k]}{dx}
    \right )
$

$E_z[i,j,k]=E_z[i,j,k] +
    \frac{dt}{\varepsilon\varepsilon_0}
    \left (
        \frac{H_y[i,j,k] - H_y[i-1,j,k]}{dx} 
      - \frac{H_x[i,j,k] - H_x[i,j-1,k]}{dy}
    \right )
$

#### Конечные приращения

$E_x[i,j,k]$+=$
    \frac{dt}{\varepsilon\varepsilon_0}
    \left (
        \frac{H_z[i,j,k] - H_z[i,j-1,k]}{dy} 
      - \frac{H_y[i,j,k] - H_y[i,j,k-1]}{dz}
    \right )
$

$E_y[i,j,k]$+=$
    \frac{dt}{\varepsilon\varepsilon_0}
    \left (
        \frac{H_x[i,j,k] - H_x[i,j,k-1]}{dz} 
      - \frac{H_z[i,j,k] - H_z[i-1,j,k]}{dx}
    \right )
$

$E_z[i,j,k]$+=$
    \frac{dt}{\varepsilon\varepsilon_0}
    \left (
        \frac{H_y[i,j,k] - H_y[i-1,j,k]}{dx} 
      - \frac{H_x[i,j,k] - H_x[i,j-1,k]}{dy}
    \right )
$

### Магнитное поле

$\frac{\partial \vec\mathbf B}{\partial t}=-rot\vec\mathbf E$

Разложим вектора на компоненты и развернём индукцию

$
\mu\mu_0\frac{\partial \mathbf H_x}{\partial t} = -
    \left (
        \frac{\partial \mathbf E_z}{\partial y} 
      - \frac{\partial \mathbf E_y}{\partial z}
    \right )
$

$
\mu\mu_0\frac{\partial \mathbf H_y}{\partial t} = -
    \left (
        \frac{\partial \mathbf E_x}{\partial z} 
        - \frac{\partial \mathbf E_z}{\partial x}
    \right ) 
$

$
\mu\mu_0\frac{\partial \mathbf H_z}{\partial t} = -
    \left (
        \frac{\partial \mathbf E_y}{\partial x} 
        - \frac{\partial \mathbf E_x}{\partial y}
    \right )
$

#### Конечные разности

$
\frac{H_x^t-H_x^{t-dt}}{dt}[i,j,k] =
   -\frac{1}{\mu\mu_0}
    \left (
        \frac{E_z[i,j,k] - E_z[i,j-1,k]}{dy} 
      - \frac{E_y[i,j,k] - E_y[i,j,k-1]}{dz}
    \right )
$

$
\frac{H_y^t-H_y^{t-dt}}{dt}[i,j,k] =
   -\frac{1}{\mu\mu_0}
    \left (
        \frac{E_x[i,j,k] - E_x[i,j,k-1]}{dz} 
      - \frac{E_z[i,j,k] - E_z[i-1,j,k]}{dx}
    \right )
$

$
\frac{H_z^t-H_z^{t-dt}}{dt}[i,j,k] =
   -\frac{1}{\mu\mu_0}
    \left (
        \frac{E_y[i,j,k] - E_y[i-1,j,k]}{dx} 
      - \frac{E_x[i,j,k] - E_x[i,j-1,k]}{dy}
    \right )
$

#### Приращения

$H_x[i,j,k] = H_x[i,j,k] -
    \frac{dt}{\mu\mu_0}
    \left (
        \frac{E_z[i,j,k] - E_z[i,j-1,k]}{dy} 
      - \frac{E_y[i,j,k] - E_y[i,j,k-1]}{dz}
    \right )
$

$H_y[i,j,k] = H_y[i,j,k] -
    \frac{dt}{\mu\mu_0}
    \left (
        \frac{E_x[i,j,k] - E_x[i,j,k-1]}{dz} 
      - \frac{E_z[i,j,k] - E_z[i-1,j,k]}{dx}
    \right )
$

$H_z[i,j,k] = H_z[i,j,k] -
    \frac{dt}{\mu\mu_0}
    \left (
        \frac{E_y[i,j,k] - E_y[i-1,j,k]}{dx} 
      - \frac{E_x[i,j,k] - E_x[i,j-1,k]}{dy}
    \right )
$

#### Конечные приращения

$H_x[i,j,k]$-=$
    \frac{dt}{\mu\mu_0}
    \left (
        \frac{E_z[i,j,k] - E_z[i,j-1,k]}{dy} 
      - \frac{E_y[i,j,k] - E_y[i,j,k-1]}{dz}
    \right )
$

$H_y[i,j,k]$-=$
    \frac{dt}{\mu\mu_0}
    \left (
        \frac{E_x[i,j,k] - E_x[i,j,k-1]}{dz} 
      - \frac{E_z[i,j,k] - E_z[i-1,j,k]}{dx}
    \right )
$

$H_z[i,j,k]$-=$
    \frac{dt}{\mu\mu_0}
    \left (
        \frac{E_y[i,j,k] - E_y[i-1,j,k]}{dx} 
      - \frac{E_x[i,j,k] - E_x[i,j-1,k]}{dy}
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
$ += $\frac{dt}{\varepsilon\varepsilon_0}
\begin{bmatrix}
    \frac{H_z[i,j,k] - H_z[i,j-1,k]}{dy} - \frac{H_y[i,j,k] - H_y[i,j,k-1]}{dz}
\\
    \frac{H_x[i,j,k] - H_x[i,j,k-1]}{dz} - \frac{H_z[i,j,k] - H_z[i-1,j,k]}{dx}
\\
    \frac{H_y[i,j,k] - H_y[i-1,j,k]}{dx} - \frac{H_x[i,j,k] - H_x[i,j-1,k]}{dy}
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
$ -= $\frac{dt}{\mu\mu_0}
\begin{bmatrix}
    \frac{E_z[i,j,k] - E_z[i,j-1,k]}{dy} - \frac{E_y[i,j,k] - E_y[i,j,k-1]}{dz}
\\
    \frac{E_x[i,j,k] - E_x[i,j,k-1]}{dz} - \frac{E_z[i,j,k] - E_z[i-1,j,k]}{dx}
\\
    \frac{E_y[i,j,k] - E_y[i-1,j,k]}{dx} - \frac{E_x[i,j,k] - E_x[i,j-1,k]}{dy}
\end{bmatrix}
$

## Частные случаи

### Двумерное пространство

#### Плоскость XOY
Приращение dz отсутствует (удаляем разность по индексу k):

$
\vec E[i,j]=\begin{bmatrix}
E_x
\\
E_y
\\
E_z
\end{bmatrix}[i,j]
$ += $\frac{dt}{\varepsilon\varepsilon_0}
\begin{bmatrix}
    \frac{H_z[i,j] - H_z[i,j-1]}{dy}
\\
   -\frac{H_z[i,j] - H_z[i-1,j]}{dx}
\\
    \frac{H_y[i,j] - H_y[i-1,j]}{dx} - \frac{H_x[i,j] - H_x[i,j-1]}{dy}
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
$ -= $\frac{dt}{\mu\mu_0}
\begin{bmatrix}
    \frac{E_z[i,j] - E_z[i,j-1]}{dy}
\\
   -\frac{E_z[i,j] - E_z[i-1,j]}{dx}
\\
    \frac{E_y[i,j] - E_y[i-1,j]}{dx} - \frac{E_x[i,j] - E_x[i,j-1]}{dy}
\end{bmatrix}
$

#### Плоскость XOZ

Приращение dy отсутствует (удаляем разность по индексу j):

$
\vec E[i,k]=\begin{bmatrix}
E_x
\\
E_y
\\
E_z
\end{bmatrix}[i,k]
$ += $\frac{dt}{\varepsilon\varepsilon_0}
\begin{bmatrix}
   -\frac{H_y[i,j,k] - H_y[i,j,k-1]}{dz}
\\
    \frac{H_x[i,k] - H_x[i,k-1]}{dz} - \frac{H_z[i,k] - H_z[i-1,k]}{dx}
\\
    \frac{H_y[i,k] - H_y[i-1,k]}{dx}
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
$ -= $\frac{dt}{\mu\mu_0}
\begin{bmatrix}
   -\frac{E_y[i,j,k] - E_y[i,k-1]}{dz}
\\
    \frac{E_x[i,k] - E_x[i,k-1]}{dz} - \frac{E_z[i,k] - E_z[i-1,j,k]}{dx}
\\
    \frac{E_y[i,k] - E_y[i-1,k]}{dx}
\end{bmatrix}
$

#### Плоскость YOZ

Приращение dx отсутствует (удаляем разность по индексу i):

$
\vec E[j,k]=\begin{bmatrix}
E_x
\\
E_y
\\
E_z
\end{bmatrix}[j,k]
$ += $\frac{dt}{\varepsilon\varepsilon_0}
\begin{bmatrix}
    \frac{H_z[j,k] - H_z[j-1,k]}{dy} - \frac{H_y[j,k] - H_y[j,k-1]}{dz}
\\
    \frac{H_x[j,k] - H_x[j,k-1]}{dz}
\\
   -\frac{H_x[j,k] - H_x[j-1,k]}{dy}
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
$ -= $\frac{dt}{\mu\mu_0}
\begin{bmatrix}
    \frac{E_z[j,k] - E_z[j-1,k]}{dy} - \frac{E_yj,k] - E_y[j,k-1]}{dz}
\\
    \frac{E_x[j,k] - E_x[j,k-1]}{dz}
\\
   -\frac{E_x[j,k] - E_x[j-1,k]}{dy}
\end{bmatrix}
$

### Одномерное пространство

#### Ось OX

Исключаем приращения dy и dz (индексы j и k):

$
\begin{bmatrix}
E_x
\\
E_y
\\
E_z
\end{bmatrix}[i]
$ += $\frac{1}{\varepsilon\varepsilon_0}\frac{dt}{dx}
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
$ -= $\frac{1}{\mu\mu_0}\frac{dt}{dx}
\begin{bmatrix}
    0
\\
    E_z[i-1] - E_z[i]
\\
    E_y[i] - E_y[i-1]
\end{bmatrix}
$

#### Ось OY

Исключаем приращения dx и dz (индексы i и k):

$
\begin{bmatrix}
E_x
\\
E_y
\\
E_z
\end{bmatrix}[j]
$ += $\frac{1}{\varepsilon\varepsilon_0}\frac{dt}{dy}
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
$ -= $\frac{1}{\mu\mu_0}\frac{dt}{dy}
\begin{bmatrix}
    E_z[j] - E_z[j-1]
\\
    0
\\
    E_x[j-1] - E_x[j]
\end{bmatrix}
$

#### Ось OZ

Исключаем приращения dx и dy (индексы i и j):

$
\begin{bmatrix}
E_x
\\
E_y
\\
E_z
\end{bmatrix}[k]
$ += $\frac{1}{\varepsilon\varepsilon_0}\frac{dt}{dz}
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
$ -= $\frac{1}{\mu\mu_0}\frac{dt}{dz}
\begin{bmatrix}
    E_y[k-1] - E_y[k]
\\
    E_x[k] - E_x[k-1]
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
$ += $\frac{1}{\varepsilon\varepsilon_0}\frac{dt}{dx}
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
$ -= $\frac{1}{\mu\mu_0}\frac{dt}{dx}
\begin{bmatrix}
    0
\\
    0
\\
    E_y[i] - E_y[i-1]
\end{bmatrix}
$

$E_y[i]+=\frac{1}{\varepsilon\varepsilon_0}\frac{dt}{dx}(H_z[i-1] - H_z[i])$

$H_z[i]+=\frac{1}{\mu\mu_0}\frac{dt}{dx}(E_y[i-1] - E_y[i])$