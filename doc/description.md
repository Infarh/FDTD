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
\frac{\partial \mathbf H_z}{\partial y} - \frac{\partial \mathbf H_y}{\partial z}\\ 
\frac{\partial \mathbf H_x}{\partial z} - \frac{\partial \mathbf H_z}{\partial x}\\ 
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
\frac{\partial \mathbf E_z}{\partial y} - \frac{\partial \mathbf E_y}{\partial z}\\ 
\frac{\partial \mathbf E_x}{\partial z} - \frac{\partial \mathbf E_z}{\partial x}\\ 
\frac{\partial \mathbf E_y}{\partial x} - \frac{\partial \mathbf E_x}{\partial y}
\end{bmatrix}
$