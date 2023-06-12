import numpy as np

def RBF_kernel(xn, xm, l = 1):
    """
    Inputs:
        xn: row n of x
        xm: row m of x
        l:  kernel hyperparameter, set to 1 by default
    Outputs:
        K:  kernel matrix element: K[n, m] = k(xn, xm)
    """
    K = np.exp(-np.linalg.norm(xn - xm)**2 / (2 * l**2))
    return K

def make_RBF_kernel(X, l = 1, sigma = 0):
    """
    Inputs:
        X: set of φ rows of inputs
        l: kernel hyperparameter, set to 1 by default
        sigma: Gaussian noise std dev, set to 0 by default
    Outputs:
        K:  Covariance matrix 
    """
    K = np.zeros([len(X), len(X)])
    for i in range(len(X)):
        for j in range(len(X)):
            K[i, j] = RBF_kernel(X[i], X[j], l)
    return K + sigma * np.eye(len(K))

def gaussian_process_predict_mean(X, y, X_new):
    """
    Inputs:
        X: set of φ rows of inputs
        y: set of φ observations 
        X_new: new input 
    Outputs:
        y_new: predicted target corresponding to X_new
    """
    rbf_kernel = make_RBF_kernel(np.vstack([X, X_new]))
    K = rbf_kernel[:len(X), :len(X)]
    k = rbf_kernel[:len(X), -1]
    return  np.dot(np.dot(k, np.linalg.inv(K)), y)

def gaussian_process_predict_std(X, X_new):
    """
    Inputs:
        X: set of φ rows of inputs
        X_new: new input
    Outputs:
        y_std: std dev. corresponding to X_new
    """
    rbf_kernel = make_RBF_kernel(np.vstack([X, X_new]))
    K = rbf_kernel[:len(X), :len(X)]
    k = rbf_kernel[:len(X), -1]
    return rbf_kernel[-1,-1] - np.dot(np.dot(k,np.linalg.inv(K)),k)


X = np.array([1995, 1996, 1997, 1998, 1999, 2000, 2001, 2002, 2003, 2004, 2005, 2006, 2007, 2008, 2009, 2010, 2011, 2012, 2013, 2014, 2015, 2016, 2017, 2018, 2019, 2020, 2021])
y = np.array([44, 45, 47, 48, 49, 48, 48, 49, 50, 52, 52, 52, 54, 56, 60, 63, 66, 67, 67, 68, 69, 69, 70, 71, 73, 76, 79])
X = X.reshape(-1, 1)
# New input to predict:
X_new = np.array([2022])
# Calculate and print the new predicted value of y:
mean_pred = gaussian_process_predict_mean(X, y, X_new)
print("mean predict :{}".format(mean_pred))
# Calculate and print the corresponding standard deviation:
sigma_pred = np.sqrt(gaussian_process_predict_std(X, X_new))
print("std predict :{}".format(sigma_pred))


for i in make_RBF_kernel(X):
    for j in i:
        print("{:.3f}".format(j), end = ", ")
    print()