from skimage.io import imread, imshow
from skimage.feature import match_template, peak_local_max
import matplotlib.pyplot as plt
import matplotlib.patches as patches
from os import listdir

make_plots = False

input_folders = ["hard", "evil"]
output_folder = "results"

# hardcoded for converting to grid coordinates
x0 = 26
y0 = 115
sq_size = 34 # px

mine_path = "mine.png"
mine = imread(mine_path, as_gray=True)

open_sq_path = "blank_square.png"
open_sq = imread(open_sq_path, as_gray=True)

for folder in input_folders:

    if folder == "hard":
        num_mines = 99
        board_width = 30
        board_height = 16
    elif folder == "evil":
        num_mines = 130
        board_width = 30
        board_height = 20
    else:
        raise Exception("Unrecognized difficuly level")

    board_files = listdir(".\\" + folder)

    for filename in board_files:

        board_path = folder + "\\" + filename
        board = imread(board_path, as_gray=True)

        print("Processing", board_path)

        mine_template_matched = match_template(board, mine, pad_input=True)
        mine_locs = peak_local_max(mine_template_matched, min_distance=20, num_peaks=num_mines)

        open_sq_template_matched = match_template(board, open_sq, pad_input=True)
        open_sq_locs = peak_local_max(open_sq_template_matched, min_distance=20, num_peaks=1)

        start_x = (open_sq_locs[0,1] - x0) // sq_size
        start_y = (open_sq_locs[0,0] - y0) // sq_size
        start_y = board_height - start_y - 1          # flip vertically
        start_sq = (start_x, start_y)

        if make_plots:
            fig, axes = plt.subplots(1,2, figsize=(6,3), sharex=True, sharey=True)
            ax = axes.ravel()

            ax[0].imshow(mine_template_matched)
            ax[0].axis('off')
            ax[0].set_title('Template Matched')

            ax[1].imshow(board, cmap = plt.cm.gray)
            ax[1].autoscale(False)
            ax[1].plot(mine_locs[:,1], mine_locs[:,0], 'r.')
            ax[1].axis('off')
            ax[1].set_title('Board with Local Maxima')

            for xx in range(0, board_width):
                for yy in range(0, board_height):
                    rect = patches.Rectangle((x0 + xx * sq_size, y0 + yy * sq_size), sq_size, sq_size, linewidth=1, edgecolor='b', facecolor='none')
                    ax[1].add_patch(rect)

            ax[1].plot(open_sq_locs[:,1], open_sq_locs[:,0], 'g.')

            plt.show()

        mines = list()
        for ii in range(0, len(mine_locs[:,1])):
            x_grid = (mine_locs[ii,1] - x0) // sq_size
            y_grid = (mine_locs[ii,0] - y0) // sq_size
            y_grid = board_height - y_grid - 1          # flip vertically
            mines.append((x_grid, y_grid))

        # Write to output file
        outfile_path = output_folder + "\\" + board_path.replace(".png", ".in")
        outfile = open(outfile_path, "w")
        outfile.write("Board Width: " + str(board_width) + "\n")
        outfile.write("Board Height: " + str(board_height) + "\n")
        outfile.write("Number of Mines: " + str(num_mines) + "\n")
        outfile.write("Starting Square: " + str(start_sq) + "\n")
        outfile.write("Mines: " + str(mines) + "\n")

print("done")
