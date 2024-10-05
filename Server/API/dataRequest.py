from astroquery.gaia import Gaia
import numpy as np
import sys

def main(args):
    if len(args) != 3:
        print("Invalid params")

    filePath = args[1]
    query = args[2]

    job = Gaia.launch_job(query)
    results = job.get_results()
    
    distance = (1 / results['parallax']) * 1000

    ra_rad = np.radians(results['ra'])
    dec_rad = np.radians(results['dec'])

    cos_ra_rad = np.cos(ra_rad)
    sin_ra_rad = np.sin(ra_rad)

    cos_dec_rad = np.cos(dec_rad)
    sin_dec_rad = np.sin(dec_rad)

    results['x'] = distance * cos_dec_rad * cos_ra_rad
    results['y'] = distance * cos_dec_rad * sin_ra_rad
    results['z'] = distance * sin_dec_rad

    results.remove_column('ra')
    results.remove_column('dec')

    results.write(filePath, format='csv', overwrite=True)
    print("SUCCESS")


if __name__ == "__main__":
    main(sys.argv)