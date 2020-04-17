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
      - \frac{H_z[i,j,k] - H_z[i,j,k-1]}{dz}
    \right )
$

$
\frac{E_y^t-E_y^{t-dt}}{dt}[i,j,k] =
    \frac{1}{\varepsilon\varepsilon_0}
    \left (
        \frac{H_z[i,j,k] - H_z[i,j,k-1]}{dz} 
      - \frac{H_z[i,j,k] - H_z[i-1,j,k]}{dx}
    \right )
$

$
\frac{E_z^t-E_z^{t-dt}}{dt}[i,j,k] =
    \frac{1}{\varepsilon\varepsilon_0}
    \left (
        \frac{H_z[i,j,k] - H_z[i-1,j,k]}{dx} 
      - \frac{H_z[i,j,k] - H_z[i,j-1,k]}{dy}
    \right )
$

#### Приращения

$E_x[i,j,k] = E_x[i,j,k] +
    \frac{dt}{\varepsilon\varepsilon_0}
    \left (
        \frac{H_z[i,j,k] - H_z[i,j-1,k]}{dy} 
      - \frac{H_z[i,j,k] - H_z[i,j,k-1]}{dz}
    \right )
$

$E_y[i,j,k] = E_y[i,j,k] +
    \frac{dt}{\varepsilon\varepsilon_0}
    \left (
        \frac{H_z[i,j,k] - H_z[i,j,k-1]}{dz} 
      - \frac{H_z[i,j,k] - H_z[i-1,j,k]}{dx}
    \right )
$

$E_z[i,j,k]=E_z[i,j,k] +
    \frac{dt}{\varepsilon\varepsilon_0}
    \left (
        \frac{H_z[i,j,k] - H_z[i-1,j,k]}{dx} 
      - \frac{H_z[i,j,k] - H_z[i,j-1,k]}{dy}
    \right )
$

#### Конечные приращения

$E_x[i,j,k]$+=$
    \frac{dt}{\varepsilon\varepsilon_0}
    \left (
        \frac{H_z[i,j,k] - H_z[i,j-1,k]}{dy} 
      - \frac{H_z[i,j,k] - H_z[i,j,k-1]}{dz}
    \right )
$

$E_y[i,j,k]$+=$
    \frac{dt}{\varepsilon\varepsilon_0}
    \left (
        \frac{H_z[i,j,k] - H_z[i,j,k-1]}{dz} 
      - \frac{H_z[i,j,k] - H_z[i-1,j,k]}{dx}
    \right )
$

$E_z[i,j,k]$+=$
    \frac{dt}{\varepsilon\varepsilon_0}
    \left (
        \frac{H_z[i,j,k] - H_z[i-1,j,k]}{dx} 
      - \frac{H_z[i,j,k] - H_z[i,j-1,k]}{dy}
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
      - \frac{E_z[i,j,k] - E_z[i,j,k-1]}{dz}
    \right )
$

$
\frac{H_y^t-H_y^{t-dt}}{dt}[i,j,k] =
   -\frac{1}{\mu\mu_0}
    \left (
        \frac{E_z[i,j,k] - E_z[i,j,k-1]}{dz} 
      - \frac{E_z[i,j,k] - E_z[i-1,j,k]}{dx}
    \right )
$

$
\frac{H_z^t-H_z^{t-dt}}{dt}[i,j,k] =
   -\frac{1}{\mu\mu_0}
    \left (
        \frac{E_z[i,j,k] - E_z[i-1,j,k]}{dx} 
      - \frac{E_z[i,j,k] - E_z[i,j-1,k]}{dy}
    \right )
$

#### Приращения

$H_x[i,j,k] = H_x[i,j,k] -
    \frac{dt}{\mu\mu_0}
    \left (
        \frac{E_z[i,j,k] - E_z[i,j-1,k]}{dy} 
      - \frac{E_z[i,j,k] - E_z[i,j,k-1]}{dz}
    \right )
$

$H_y[i,j,k] = H_y[i,j,k] -
    \frac{dt}{\mu\mu_0}
    \left (
        \frac{E_z[i,j,k] - E_z[i,j,k-1]}{dz} 
      - \frac{E_z[i,j,k] - E_z[i-1,j,k]}{dx}
    \right )
$

$H_z[i,j,k] = H_z[i,j,k] -
    \frac{dt}{\mu\mu_0}
    \left (
        \frac{E_z[i,j,k] - E_z[i-1,j,k]}{dx} 
      - \frac{E_z[i,j,k] - E_z[i,j-1,k]}{dy}
    \right )
$

#### Конечные приращения

$H_x[i,j,k]$-=$
    \frac{dt}{\mu\mu_0}
    \left (
        \frac{E_z[i,j,k] - E_z[i,j-1,k]}{dy} 
      - \frac{E_z[i,j,k] - E_z[i,j,k-1]}{dz}
    \right )
$

$H_y[i,j,k]$-=$
    \frac{dt}{\mu\mu_0}
    \left (
        \frac{E_z[i,j,k] - E_z[i,j,k-1]}{dz} 
      - \frac{E_z[i,j,k] - E_z[i-1,j,k]}{dx}
    \right )
$

$H_z[i,j,k]$-=$
    \frac{dt}{\mu\mu_0}
    \left (
        \frac{E_z[i,j,k] - E_z[i-1,j,k]}{dx} 
      - \frac{E_z[i,j,k] - E_z[i,j-1,k]}{dy}
    \right )
$